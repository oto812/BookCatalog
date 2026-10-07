// BookCatalog infrastructure: everything built by hand in Phase 1 and 2, except data.
//
// Deployed in two stages, because the Container App can only start once the registry has an
// image and the vault has the connection string - and both of those are filled in by hand:
//
//   stage 1  deployApp=false   identity, logs, registry, vault, Postgres, environment, roles
//   (by hand)                  push image, run migrations, create the app DB user, add the secret
//   stage 2  deployApp=true    the Container App
//
// See infra/README.md for the exact commands.

targetScope = 'resourceGroup'

// ---------------------------------------------------------------------------------------------
// Parameters: the values that differ between deployments. Everything else is fixed below.
// ---------------------------------------------------------------------------------------------

@description('Azure region for every resource. Defaults to the resource group\'s region.')
param location string = resourceGroup().location

@description('Makes the globally unique names (registry, vault, Postgres) unique to you.')
param suffix string = 'oto'

@description('Postgres superuser. Used by you for migrations, never by the app.')
param postgresAdminLogin string = 'bookadmin'

@secure()
@description('Postgres superuser password. Not stored anywhere in this repo - az asks for it.')
param postgresAdminPassword string

@description('Your public IP, so your laptop can reach Postgres to run migrations.')
param clientIp string

@description('Your Entra object ID, so you can manage secrets in the vault (data plane).')
param deployerObjectId string

@description('false for stage 1, true for stage 2. See the header.')
param deployApp bool = false

@description('Which image tag the Container App runs.')
param imageTag string = 'v1'

@description('The GitHub repository whose pipeline may deploy, as owner/name.')
param githubRepo string = 'oto812@64520066/BookCatalog@1334074172'

// ---------------------------------------------------------------------------------------------
// Names and built-in role IDs
// ---------------------------------------------------------------------------------------------

var names = {
  identity: 'id-bookcatalog-api'
  logs: 'log-bookcatalog'
  environment: 'cae-bookcatalog'
  app: 'ca-bookcatalog-api'
  registry: 'acrbookcatalog${suffix}' // letters and digits only
  vault: 'kv-bookcatalog-${suffix}'
  postgres: 'pg-bookcatalog-${suffix}'
  database: 'bookcatalog'
  deployIdentity: 'id-bookcatalog-deploy'
  githubMainBranch: 'github-main'
}

// Built-in roles have the same ID in every Azure tenant.
// Look one up with: az role definition list --name "AcrPull" --query "[0].name"
var roles = {
  acrPull: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
  keyVaultSecretsUser: '4633458b-17de-408a-b874-0445c86b69e6'
  keyVaultSecretsOfficer: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
  acrPush: '8311e382-0749-4cb8-b61a-304f252e45ec'
}

// ---------------------------------------------------------------------------------------------
// Identity: the app's badge. User-assigned, so it exists before the app does.
// ---------------------------------------------------------------------------------------------

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: names.identity
  location: location
}


resource deployIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: names.deployIdentity
  location: location
}


resource githubMainBranch 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: deployIdentity
  name: names.githubMainBranch
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'    // signed by GitHub Actions
    subject: 'repo:${githubRepo}:ref:refs/heads/main'         // this repo, main branch only
    audiences: [
      'api://AzureADTokenExchange'                             // meant for Entra
    ]
  }
}

// ---------------------------------------------------------------------------------------------
// Logs: where the Container Apps environment sends console output.
// ---------------------------------------------------------------------------------------------

resource logs 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: names.logs
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ---------------------------------------------------------------------------------------------
// Registry: stores the image. No admin user - the app pulls with its identity (AcrPull).
// ---------------------------------------------------------------------------------------------

resource registry 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: names.registry
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

// ---------------------------------------------------------------------------------------------
// Key Vault: the one home for secrets. Only the vault is described here - the secret VALUES
// are added by hand, so no password ever appears in this file or in deployment history.
// ---------------------------------------------------------------------------------------------

resource vault 'Microsoft.KeyVault/vaults@2025-05-01' = {
  name: names.vault
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true // permissions through Azure RBAC, not the older access policies
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    // Purge protection is left off on purpose, so a deleted vault can be purged and its name
    // reused when the environment is rebuilt. Turn it on for anything real - it cannot be
    // turned off again once enabled.
    publicNetworkAccess: 'Enabled'
  }
}

// ---------------------------------------------------------------------------------------------
// Postgres: the stateful part. This describes the SERVER, not what is inside it - redeploying
// leaves tables and rows alone, and deleting it deletes them with no way back.
// ---------------------------------------------------------------------------------------------

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2025-08-01' = {
  name: names.postgres
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '17'
    administratorLogin: postgresAdminLogin
    // Sent on every deployment: a different value here CHANGES the admin password.
    administratorLoginPassword: postgresAdminPassword
    authConfig: {
      passwordAuth: 'Enabled'
      activeDirectoryAuth: 'Disabled'
    }
    storage: {
      storageSizeGB: 32 // can grow later, never shrink
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled' // fixed at creation; private access would need a new server
    }
  }
}

// Postgres rejects changes that arrive at the same time, so these three run one after another
// (dependsOn) instead of in parallel, which is what Bicep would otherwise do.

// The special 0.0.0.0 rule means "any IP that belongs to Azure" - including other customers.
// Needed because Consumption apps have no fixed outbound IP. See the Phase 1 networking notes.
resource allowAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2025-08-01' = {
  parent: postgres
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowClient 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2025-08-01' = {
  parent: postgres
  name: 'AllowClientIp'
  properties: {
    startIpAddress: clientIp
    endIpAddress: clientIp
  }
  dependsOn: [
    allowAzure
  ]
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2025-08-01' = {
  parent: postgres
  name: names.database
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
  dependsOn: [
    allowClient
  ]
}

// ---------------------------------------------------------------------------------------------
// Role assignments: WHO + WHAT + WHERE.
// The name is guid(where, who, what), so the same inputs always give the same name - that is
// what makes redeploying idempotent instead of failing with "role assignment already exists".
// ---------------------------------------------------------------------------------------------

// The app identity may pull images - from this registry only.
resource identityAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, identity.id, roles.acrPull)
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.acrPull)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// The app identity may read secret values - from this vault only.
resource identitySecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vault.id, identity.id, roles.keyVaultSecretsUser)
  scope: vault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// You may create and change secrets. Being Owner is not enough: Owner covers the control plane,
// reading and writing secrets is the data plane.
resource deployerSecretsOfficer 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vault.id, deployerObjectId, roles.keyVaultSecretsOfficer)
  scope: vault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsOfficer)
    principalId: deployerObjectId
    principalType: 'User'
  }
}

resource deployAcrPush 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, deployIdentity.id, roles.acrPush)
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.acrPush)
    principalId: deployIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------------------------------------
// Container Apps environment: the neighborhood - shared network and logging.
// ---------------------------------------------------------------------------------------------

resource environment 'Microsoft.App/managedEnvironments@2025-07-01' = {
  name: names.environment
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    zoneRedundant: false
  }
}

// ---------------------------------------------------------------------------------------------
// Container App: the stateless part. Only created in stage 2 (deployApp=true).
// ---------------------------------------------------------------------------------------------

resource app 'Microsoft.App/containerApps@2025-07-01' = if (deployApp) {
  name: names.app
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    environmentId: environment.id
    workloadProfileName: 'Consumption'
    configuration: {
      // App-level settings: changing these does NOT create a new revision.
      ingress: {
        external: true // false = only reachable from inside the environment (the .internal URL)
        targetPort: 8080 // must match ASPNETCORE_HTTP_PORTS
        transport: 'auto'
        allowInsecure: false // http:// is redirected to https://
      }
      registries: [
        {
          server: registry.properties.loginServer
          identity: identity.id // pull with the app's own identity, not a password
        }
      ]
      secrets: [
        {
          name: 'db-connection'
          // No version at the end: the newest version is used, so rotating the secret in the
          // vault reaches the app on its next restart.
          keyVaultUrl: '${vault.properties.vaultUri}secrets/db-connection'
          identity: identity.id
        }
      ]
    }
    template: {
      // The template is what a revision snapshots: changing anything here creates a new one.
      containers: [
        {
          name: 'bookcatalog-api'
          image: '${registry.properties.loginServer}/bookcatalog-api:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8080'
            }
            {
              name: 'ConnectionStrings__BookCatalog'
              secretRef: 'db-connection'
            }
          ]
          // timeoutSeconds defaults to 1 when left out (the portal fills in 5). The first
          // connection to Postgres - TCP, TLS and login - takes longer than 1 second, so with the
          // default the readiness probe gives up before it finishes, cancels it, and the next
          // probe starts from scratch: the revision never becomes ready.
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
              }
              timeoutSeconds: 5
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              timeoutSeconds: 5
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0 // scale to zero when idle: cheap, but the first request is a cold start
        maxReplicas: 10
      }
    }
  }
  dependsOn: [
    identityAcrPull
    identitySecretsUser
  ]
}

// ---------------------------------------------------------------------------------------------
// Outputs: printed after deployment, for the manual steps in between the two stages.
// ---------------------------------------------------------------------------------------------

output registryLoginServer string = registry.properties.loginServer
output postgresHost string = postgres.properties.fullyQualifiedDomainName
output vaultName string = vault.name
output appUrl string = deployApp ? 'https://${app!.properties.configuration.ingress.fqdn}' : 'not deployed yet (stage 1)'
output deployIdentityClientId string = deployIdentity.properties.clientId