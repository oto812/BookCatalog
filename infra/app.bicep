
param location string = resourceGroup().location

param suffix string = 'oto'

param appName string = 'ca-bookcatalog-api'

param imageTag string

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
    name: 'id-book-api'
}


resource registry 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: 'acrbookcatalog${suffix}'
}

resource vault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: 'kv-bookcatalog-${suffix}'  
}


resource environment 'Microsoft.App/managedEnvironments@2025-07-01' existing = {
  name: 'cae-bookcatalog'
}



resource app 'Microsoft.App/containerApps@2025-07-01' = {
  name: appName
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
}
