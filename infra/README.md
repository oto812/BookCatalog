# Infrastructure

`main.bicep` describes the whole Azure environment: managed identity, Log Analytics,
Container Registry, Key Vault, PostgreSQL Flexible Server, the Container Apps environment,
the role assignments between them, and the Container App itself.

It describes infrastructure, not data. What it leaves out, and why, is listed at the end.

Commands are PowerShell, run from the repository root.

---

## Once: tools

```powershell
az bicep install
az bicep build --file infra/main.bicep
```

`build` compiles the file to an ARM template and reports mistakes before anything reaches Azure.

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

### 1. Stage 1: everything except the app

```powershell
az group create --name rg-bookCatalog --location swedencentral

$myIp = curl.exe -s https://api.ipify.org
$me = az ad signed-in-user show --query id -o tsv

az deployment group what-if -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me
az deployment group create  -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me
```

`az` prompts for `postgresAdminPassword`, so it never appears in the command or the shell
history. Use the same password on every later deployment: whatever is entered becomes the
admin password.

`what-if` shows what would change without changing anything. Read it before every `create`.

### 2. By hand: image, schema, app user, secret

**Image.** The registry is new and empty.

```powershell
az acr login --name acrbookcatalogoto
docker build --platform linux/amd64 -t acrbookcatalogoto.azurecr.io/bookcatalog-api:v1 -f BookCatalog.Api/Dockerfile .
docker push acrbookcatalogoto.azurecr.io/bookcatalog-api:v1
```

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

**Secret.** In the portal: Key vault → Secrets → Generate/Import, name `db-connection`:

```
Host=pg-bookcatalog-oto.postgres.database.azure.com;Port=5432;Database=bookcatalog;Username=bookcatalog_app;Password=<app-password>;SSL Mode=Require
```

### 3. Stage 2: the app

```powershell
az deployment group create -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me deployApp=true
```

The output `appUrl` is the address. Check it:

```powershell
curl.exe https://<appUrl>/health/ready
curl.exe "https://<appUrl>/api/books?page=1&pageSize=5"
```

---

## Deploying again

Every later deployment is the stage 2 command. Run `what-if` with the same parameters first:
with nothing changed in the file, it should report no changes to speak of. Deploying twice
gives the same result as deploying once.

A new image version is a new tag:

```powershell
docker build --platform linux/amd64 -t acrbookcatalogoto.azurecr.io/bookcatalog-api:v2 -f BookCatalog.Api/Dockerfile .
docker push acrbookcatalogoto.azurecr.io/bookcatalog-api:v2
az deployment group create -g rg-bookCatalog -f infra/main.bicep -p clientIp=$myIp deployerObjectId=$me deployApp=true imageTag=v2
```

---

## Not in the template, and why

| Left out | Why |
|---|---|
| Secret values (`db-connection`, passwords) | A template is committed to git and recorded in deployment history. Secrets go straight into Key Vault instead. |
| The image | It is built from the code and changes on every release, not when the infrastructure changes. |
| The schema (migrations) | It lives inside the database. Bicep talks to Azure Resource Manager, which manages the server, not its tables. |
| The `bookcatalog_app` user and its grants | Same reason: Postgres users are inside the database (data plane), out of Resource Manager's reach. |
| The data | Infrastructure can be recreated. Data can only be restored from a backup. |
| The budget alert | It belongs to the subscription, not this resource group. |

## If the resource group disappeared

| Comes back by running the template | Lost |
|---|---|
| Identity, registry, vault, Postgres server, environment, app, firewall rules, role assignments | Every row in the database, along with the server's automatic backups |
| | Secret values, unless the soft-deleted vault is recovered instead of purged |
| | The image, which is rebuilt from git in one command |
