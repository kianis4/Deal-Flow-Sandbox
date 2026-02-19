# Azure Container Apps Setup

## Prerequisites

```bash
brew install azure-cli
az login
```

## One-Time Infrastructure Setup

```bash
# Set variables
RESOURCE_GROUP="rg-dealflow"
LOCATION="canadacentral"
ACR_NAME="dealflowacr$(openssl rand -hex 4)"   # unique name
ACA_ENV="dealflow-env"
PG_SERVER="dealflow-pg-$(openssl rand -hex 4)"
PG_PASSWORD="Deal@Flow2026!"

# Resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Azure Container Registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Get credentials (save for GitHub Secrets)
ACR_SERVER=$(az acr show --name $ACR_NAME --query loginServer -o tsv)
ACR_USER=$(az acr credential show --name $ACR_NAME --query username -o tsv)
ACR_PASS=$(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv)

echo "ACR_LOGIN_SERVER=$ACR_SERVER"
echo "ACR_USERNAME=$ACR_USER"
echo "ACR_PASSWORD=$ACR_PASS"

# PostgreSQL Flexible Server
az postgres flexible-server create \
  --resource-group $RESOURCE_GROUP \
  --name $PG_SERVER \
  --location $LOCATION \
  --admin-user dealflow \
  --admin-password "$PG_PASSWORD" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16 \
  --yes

az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $PG_SERVER \
  --database-name dealflow

az postgres flexible-server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --name $PG_SERVER \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# RabbitMQ via Azure Container Apps (simple, no managed service needed)
# We deploy RabbitMQ as a container within the ACA environment
az containerapp env create \
  --name $ACA_ENV \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

PG_CONN="Host=$PG_SERVER.postgres.database.azure.com;Database=dealflow;Username=dealflow;Password=$PG_PASSWORD;SslMode=Require"

# Deploy RabbitMQ container
az containerapp create \
  --name rabbitmq \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image rabbitmq:3.13-management-alpine \
  --target-port 5672 \
  --ingress internal \
  --min-replicas 1 --max-replicas 1

# Deploy services (after pushing images via GitHub Actions)
az containerapp create \
  --name deal-intake-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image "$ACR_SERVER/deal-intake-api:latest" \
  --registry-server "$ACR_SERVER" \
  --registry-username "$ACR_USER" \
  --registry-password "$ACR_PASS" \
  --target-port 8080 \
  --ingress external \
  --env-vars \
    "ConnectionStrings__DefaultConnection=$PG_CONN" \
    "RabbitMQ__Host=rabbitmq" \
  --min-replicas 1 --max-replicas 3

az containerapp create \
  --name deal-reporting-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image "$ACR_SERVER/deal-reporting-api:latest" \
  --registry-server "$ACR_SERVER" \
  --registry-username "$ACR_USER" \
  --registry-password "$ACR_PASS" \
  --target-port 8080 \
  --ingress external \
  --env-vars "ConnectionStrings__DefaultConnection=$PG_CONN" \
  --min-replicas 1 --max-replicas 3

az containerapp create \
  --name deal-scoring-worker \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image "$ACR_SERVER/deal-scoring-worker:latest" \
  --registry-server "$ACR_SERVER" \
  --registry-username "$ACR_USER" \
  --registry-password "$ACR_PASS" \
  --ingress none \
  --env-vars \
    "ConnectionStrings__DefaultConnection=$PG_CONN" \
    "RabbitMQ__Host=rabbitmq" \
  --min-replicas 1 --max-replicas 1

az containerapp create \
  --name deal-notify-worker \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image "$ACR_SERVER/deal-notify-worker:latest" \
  --registry-server "$ACR_SERVER" \
  --registry-username "$ACR_USER" \
  --registry-password "$ACR_PASS" \
  --ingress none \
  --env-vars "RabbitMQ__Host=rabbitmq" \
  --min-replicas 1 --max-replicas 1
```

## GitHub Actions Secrets

In your GitHub repo -> Settings -> Secrets and variables -> Actions, add:

| Secret | Value |
|---|---|
| `ACR_LOGIN_SERVER` | Output of `$ACR_SERVER` above |
| `ACR_USERNAME` | Output of `$ACR_USER` above |
| `ACR_PASSWORD` | Output of `$ACR_PASS` above |

## Get Public URLs

```bash
az containerapp show \
  --name deal-intake-api \
  --resource-group $RESOURCE_GROUP \
  --query properties.configuration.ingress.fqdn -o tsv

az containerapp show \
  --name deal-reporting-api \
  --resource-group $RESOURCE_GROUP \
  --query properties.configuration.ingress.fqdn -o tsv
```
