# Infrastructure

The Azure environment is described by two templates, split by how often they change and
who is allowed to deploy them:

| Template | Describes | Changes | Deployed by |
|---|---|---|---|
| `main.bicep` | Identities, Log Analytics, Container Registry, Key Vault, PostgreSQL Flexible Server, the Container Apps environment, and the role assignments between them | Rarely | You, by hand |
| `app.bicep` | The Container App only | On every merge to `main` | The pipeline, with the commit SHA as the image tag |

Keeping the app out of `main.bicep` means an infrastructure deployment can never roll the
running image back to an old tag, and the pipeline never needs permission to change
infrastructure or grant roles.

Both templates describe infrastructure, not data. What they leave out, and why, is listed at
the end.

Commands are PowerShell, run from the repository root.

---

## Who can do what

| Identity | Role | Scope | Used for |
|---|---|---|---|
| `id-bookcatalog-api` | AcrPull | Registry | The app pulls its image |
| `id-bookcatalog-api` | Key Vault Secrets User | Vault | The app reads `db-connection` |
| `id-bookcatalog-deploy` | AcrPush | Registry | The pipeline pushes the tested image |
| `id-bookcatalog-deploy` | Contributor | Resource group | The pipeline deploys `app.bicep`. Contributor cannot grant roles, so the pipeline cannot give itself more access. |
| You (`deployerObjectId`) | Key Vault Secrets Officer | Vault | You create and change secrets |

The pipeline signs in without a stored secret. `id-bookcatalog-deploy` has a federated
credential that accepts GitHub's OIDC token only from this repository on `main`:

```
repo:oto812@64520066/BookCatalog@1334074172:ref:refs/heads/main
```

The numbers are GitHub's owner and repository IDs. A repository re-created under the same name
gets new IDs, so it cannot match.

---

## Once: tools

```powershell
az bicep install
az bicep build --file infra/main.bicep
az bicep build --file infra/app.bicep
```

`build` compiles a template to ARM and reports mistakes before anything reaches Azure.

---

## Changing infrastructure

Edit `main.bicep`, then:

```powershell
$myIp = curl.exe -s https://api.ipify.org
$me = az ad signed-in-user show --query id -o tsv

az deployment group what-if -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me
az deployment group create  -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me
```

`az` prompts for `postgresAdminPassword`, so it never appears in the command or the shell
history. Enter the current password every time: whatever is entered becomes the admin password.

Read `what-if` before every `create`. Most of its output is noise:

- `~ value => "[reference(...)]"` - the value comes from another resource and `what-if` does not
  look it up. It resolves to the same value.
- `- property` on a property the template never mentions - an Azure default, kept as it is.
- `x Noeffect` - a property Azure does not store.

What matters is a real change to a setting you care about, and above all any `- Delete` of a
whole resource. The app is not in `main.bicep`, but deployments run in incremental mode, which
never deletes what a template leaves out.

## Deploying the app

Merging to `main` does it. The pipeline builds the image once, tests the code and the image,
pushes that image as `bookcatalog-api:<commit sha>`, deploys `app.bicep` with it, and waits until
the new revision is the one serving traffic.

By hand, for example to put back a tag that is already in the registry:

```powershell
az acr repository show-tags -n acrbookcatalogoto --repository bookcatalog-api -o table

az deployment group what-if -g rg-bookCatalog -f infra/app.bicep -p imageTag=<tag>
az deployment group create  -g rg-bookCatalog -f infra/app.bicep -p imageTag=<tag>
```

`imageTag` has no default: a deployment that does not say which version refuses to run.

A deployment that changes nothing in the template creates no new revision. A revision that
failed to activate is not retried by itself, and `az containerapp revision restart` has no
replicas to restart. Deploy a changed template, or force a new revision:

```powershell
az containerapp update -g rg-bookCatalog -n ca-bookcatalog-api --revision-suffix <new-suffix>
```

---

## Rebuilding from nothing

### 0. Remove the old environment

```powershell
az group delete --name rg-bookCatalog --yes
```

This takes 5-15 minutes and deletes the database with its backups.

A deleted Key Vault is only soft-deleted and keeps its name reserved. Purge it so the
deployment can create a vault with the same name:

```powershell
az keyvault list-deleted --query "[].name" -o tsv
az keyvault purge --name kv-bookcatalog-oto
```

### 1. Infrastructure

```powershell
az group create --name rg-bookCatalog --location swedencentral
```

Then deploy `main.bicep` as in [Changing infrastructure](#changing-infrastructure).

### 2. Point GitHub at the new deploy identity

A re-created identity has a new client ID. Update the repository variable
`AZURE_CLIENT_ID` (Settings → Secrets and variables → Actions → Variables) with:

```powershell
az identity show -g rg-bookCatalog -n id-bookcatalog-deploy --query clientId -o tsv
```

The other variables stay the same: `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`,
`ACR_NAME` = `acrbookcatalogoto`, `RESOURCE_GROUP` = `rg-bookCatalog`. None of them is a secret:
they say which identity to use, not how to prove it.

### 3. By hand: schema, app user, secret

**Schema.** The password is read without echoing and never typed into a command, so it stays
out of the history file:

```powershell
$pw = Read-Host "bookadmin password" -AsSecureString
$plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pw))
$env:ConnectionStrings__BookCatalog = "Host=pg-bookcatalog-oto.postgres.database.azure.com;Port=5432;Database=bookcatalog;Username=bookadmin;Password=$plain;SSL Mode=Require"

dotnet ef database update --project BookCatalog.Infrastructure --startup-project BookCatalog.Api

Remove-Item Env:ConnectionStrings__BookCatalog; Remove-Variable plain, pw
```

**App database user.** psql asks for the password itself:

```powershell
docker run -it --rm postgres:17 psql "host=pg-bookcatalog-oto.postgres.database.azure.com port=5432 dbname=bookcatalog user=bookadmin sslmode=require"
```

```sql
CREATE ROLE bookcatalog_app LOGIN PASSWORD '<app-password>';
GRANT CONNECT ON DATABASE bookcatalog TO bookcatalog_app;
GRANT USAGE ON SCHEMA public TO bookcatalog_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO bookcatalog_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO bookcatalog_app;
ALTER DEFAULT PRIVILEGES FOR ROLE bookadmin IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO bookcatalog_app;
ALTER DEFAULT PRIVILEGES FOR ROLE bookadmin IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO bookcatalog_app;
\q
```

Postgres stores only a hash of each password. A forgotten password cannot be looked up, only
reset - `\password bookcatalog_app` as `bookadmin` - after which the secret below must match it.

**Secret.** In the portal: Key vault → Secrets → Generate/Import, name `db-connection`:

```
Host=pg-bookcatalog-oto.postgres.database.azure.com;Port=5432;Database=bookcatalog;Username=bookcatalog_app;Password=<app-password>;SSL Mode=Require
```

### 4. The app

In GitHub, open the latest run of the CI workflow on `main` and choose **Re-run all jobs**. The
registry is empty after a rebuild; the pipeline pushes the image and deploys `app.bicep`.

Check it:

```powershell
$fqdn = az containerapp show -g rg-bookCatalog -n ca-bookcatalog-api --query properties.configuration.ingress.fqdn -o tsv
curl.exe "https://$fqdn/health/ready"
curl.exe "https://$fqdn/api/books?page=1&pageSize=5"
```

If the revision fails to activate, its logs are in the `log-bookcatalog` workspace even when it
has no replica left to stream from:

```
ContainerAppSystemLogs_CL
| where RevisionName_s startswith "ca-bookcatalog-api"
| project TimeGenerated, RevisionName_s, Reason_s, Log_s
| order by TimeGenerated desc
```

---

## Not in the templates, and why

| Left out | Why |
|---|---|
| Secret values (`db-connection`, passwords) | A template is committed to git and recorded in deployment history. Secrets go straight into Key Vault instead. |
| The image | It is built from the code by the pipeline, on every release. |
| The schema (migrations) | It lives inside the database. Bicep talks to Azure Resource Manager, which manages the server, not its tables. |
| The `bookcatalog_app` user and its grants | Same reason: Postgres users are inside the database (data plane), out of Resource Manager's reach. |
| The data | Infrastructure can be recreated. Data can only be restored from a backup. |
| The GitHub repository variables | They live in GitHub, not Azure. |
| The budget alert | It belongs to the subscription, not this resource group. |

## If the resource group disappeared

| Comes back by running the templates | Lost |
|---|---|
| Identities, registry, vault, Postgres server, environment, app, firewall rules, role assignments, the federated credential | Every row in the database, along with the server's automatic backups |
| The schema, from the migrations in git | Secret values, unless the soft-deleted vault is recovered instead of purged |
| The image, rebuilt and pushed by re-running the pipeline | The deploy identity's client ID, which changes and must be updated in GitHub |
