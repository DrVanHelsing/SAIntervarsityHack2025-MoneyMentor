# Azure Infrastructure Deployment - FinanceBuddy/MoneyMentor

This Bicep template deploys the complete Azure infrastructure required for the FinanceBuddy MoneyMentor application, including database, AI services, and hosting platform.

## Overview

The `main.bicep` file provisions a comprehensive Azure infrastructure stack optimized for the South African region, with AI services deployed to the nearest supported regions for optimal performance and compliance.

## Architecture

### ??? Infrastructure Components

| Service | Purpose | Region | SKU |
|---------|---------|---------|-----|
| **Azure SQL Database** | Expense and user data storage | South Africa North | S0 Standard |
| **App Service (Linux)** | API hosting (.NET 8) | South Africa North | S1 Standard |
| **Azure OpenAI** | AI-powered financial advice | West Europe* | S0 Standard |
| **Speech Services** | Voice recognition for expenses | South Africa North | S0 Standard |
| **Translator Services** | Multi-language support | South Africa North | S1 Standard |
| **Azure SignalR** | Real-time features (optional) | South Africa North | S1 Standard |

*_Azure OpenAI is deployed to West Europe as it's not yet available in South African regions_

## Prerequisites

### Azure Requirements
- **Azure Subscription** with sufficient credits/budget
- **Azure CLI** installed and authenticated
- **Resource Group** created for deployment
- **Azure OpenAI Access** - Your subscription must be approved for Azure OpenAI services

### Permissions Required
- **Contributor** role on the target resource group
- **Ability to create** Cognitive Services resources
- **SQL Database** creation permissions

### Azure OpenAI Approval
?? **Important**: Azure OpenAI requires subscription approval. Apply at [Azure OpenAI Access Request](https://aka.ms/oai/access) before deployment.

## Deployment Instructions

### 1. Quick Deployment

```bash
# Set variables
RESOURCE_GROUP="financebuddy-rg"
LOCATION="southafricanorth"
SQL_ADMIN_LOGIN="sqladminuser"
SQL_ADMIN_PASSWORD="YourSecurePassword123!"
CLIENT_IP=$(curl -s https://api.ipify.org)

# Deploy infrastructure
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file scripts/main.bicep \
  --parameters sqlAdminLogin=$SQL_ADMIN_LOGIN \
  --parameters sqlAdminPassword=$SQL_ADMIN_PASSWORD \
  --parameters clientIp=$CLIENT_IP \
  --parameters addClientIP=true
```

### 2. Step-by-Step Deployment

#### Step 1: Create Resource Group
```bash
# Create resource group in South Africa North
az group create \
  --name financebuddy-rg \
  --location southafricanorth
```

#### Step 2: Prepare Parameters
```bash
# Get your current IP address
export CLIENT_IP=$(curl -s https://api.ipify.org)
echo "Your IP: $CLIENT_IP"

# Set secure SQL password
export SQL_PASSWORD="YourSecurePassword123!"
```

#### Step 3: Deploy with Custom Parameters
```bash
az deployment group create \
  --resource-group financebuddy-rg \
  --template-file scripts/main.bicep \
  --parameters projectName=financebuddy \
  --parameters location=southafricanorth \
  --parameters openAIlocation=westeurope \
  --parameters sqlAdminLogin=sqladminuser \
  --parameters sqlAdminPassword=$SQL_PASSWORD \
  --parameters clientIp=$CLIENT_IP \
  --parameters addClientIP=true \
  --parameters createSignalR=false
```

### 3. Advanced Deployment Options

#### Enable SignalR for Real-time Features
```bash
az deployment group create \
  --resource-group financebuddy-rg \
  --template-file scripts/main.bicep \
  --parameters createSignalR=true \
  # ... other parameters
```

#### Deploy to Different Regions
```bash
# Deploy to West Europe (for testing)
az deployment group create \
  --resource-group financebuddy-eu-rg \
  --template-file scripts/main.bicep \
  --parameters location=westeurope \
  --parameters openAIlocation=westeurope \
  # ... other parameters
```

## Parameters Reference

### Required Parameters

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `sqlAdminLogin` | string | SQL Server admin username | `sqladminuser` |
| `sqlAdminPassword` | securestring | SQL Server admin password | `SecurePass123!` |

### Optional Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `projectName` | string | `moneymentor` | Base name for all resources |
| `location` | string | `southafricanorth` | Primary deployment region |
| `openAIlocation` | string | `westeurope` | Azure OpenAI deployment region |
| `addClientIP` | bool | `true` | Add firewall rule for client IP |
| `clientIp` | string | `0.0.0.0` | Client IP address to allow |
| `createSignalR` | bool | `false` | Deploy SignalR service |

## Post-Deployment Configuration

### 1. Configure Azure OpenAI

#### Deploy GPT Model
```bash
# Get OpenAI resource name from deployment output
OPENAI_NAME=$(az deployment group show \
  --resource-group financebuddy-rg \
  --name main \
  --query 'properties.outputs.openAIResourceId.value' \
  --output tsv | cut -d'/' -f9)

# Deploy GPT-4 model (requires Azure OpenAI Studio)
echo "Deploy GPT-4 model in Azure OpenAI Studio:"
echo "Resource: $OPENAI_NAME"
echo "Model: gpt-4o"
echo "Deployment Name: gpt-4o"
```

#### Set API Key
```bash
# Get OpenAI API key
OPENAI_KEY=$(az cognitiveservices account keys list \
  --resource-group financebuddy-rg \
  --name $OPENAI_NAME \
  --query 'key1' --output tsv)

# Get App Service name
API_APP_NAME=$(az deployment group show \
  --resource-group financebuddy-rg \
  --name main \
  --query 'properties.outputs.apiUrl.value' \
  --output tsv | sed 's/https:\/\///' | sed 's/.azurewebsites.net//')

# Set OpenAI API key in App Service
az webapp config appsettings set \
  --resource-group financebuddy-rg \
  --name $API_APP_NAME \
  --settings AzureOpenAI__ApiKey=$OPENAI_KEY
```

### 2. Database Setup

#### Run Entity Framework Migrations
```bash
# From your local development environment
# Update connection string in appsettings.json with deployment output

# Apply migrations
dotnet ef database update \
  --project MoneyMentor.ApiOrchestrator \
  --connection "Server=tcp://[SQL_SERVER_NAME].database.windows.net,1433;Initial Catalog=moneymentor;Persist Security Info=False;User ID=sqladminuser;Password=[SQL_PASSWORD];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 3. Deploy Application Code

#### Deploy API to App Service
```bash
# Publish API application
dotnet publish MoneyMentor.ApiOrchestrator \
  --configuration Release \
  --output ./publish

# Deploy to Azure
az webapp deploy \
  --resource-group financebuddy-rg \
  --name $API_APP_NAME \
  --src-path ./publish \
  --type zip
```

## Resource Naming Convention

Resources are automatically named using the pattern: `{projectName}{uniqueString}[-service]`

### Example Resource Names
- **SQL Server**: `moneymentorabc123-sql`
- **Database**: `moneymentor`
- **App Service**: `moneymentorabc123-api`
- **OpenAI**: `moneymentorabc123-openai`
- **Speech Service**: `moneymentorabc123-speech`
- **App Service Plan**: `moneymentorabc123-plan`

## Outputs and Connection Information

After successful deployment, the template provides these outputs:

| Output | Description | Use Case |
|--------|-------------|----------|
| `apiUrl` | App Service URL | API endpoint for MAUI app |
| `sqlServerName` | SQL Server name | Database connections |
| `sqlConnection` | Complete connection string | App configuration |
| `speechKeyEndpoint` | Speech service endpoint | Voice recognition |
| `openAIEndpointOut` | OpenAI service endpoint | AI chat functionality |

### Retrieve Deployment Outputs
```bash
# Get all outputs
az deployment group show \
  --resource-group financebuddy-rg \
  --name main \
  --query 'properties.outputs'

# Get specific output (API URL)
az deployment group show \
  --resource-group financebuddy-rg \
  --name main \
  --query 'properties.outputs.apiUrl.value' \
  --output tsv
```

## Security Configuration

### ?? Security Features Implemented

1. **SQL Database Security**
   - TLS 1.2 minimum encryption
   - Firewall rules for Azure services
   - Optional client IP whitelisting
   - Admin login with secure password

2. **App Service Security**
   - HTTPS only enforcement
   - Linux container hosting
   - Secure app settings storage

3. **Cognitive Services Security**
   - Public network access controlled
   - Key-based authentication
   - Regional data residency compliance

### Additional Security Recommendations

```bash
# Enable Advanced Threat Protection for SQL
az sql server threat-policy update \
  --resource-group financebuddy-rg \
  --server $SQL_SERVER_NAME \
  --state Enabled

# Configure App Service authentication (optional)
az webapp auth update \
  --resource-group financebuddy-rg \
  --name $API_APP_NAME \
  --enabled true
```

## Cost Optimization

### ?? Estimated Monthly Costs (South Africa North)

| Service | SKU | Estimated Cost (USD) |
|---------|-----|---------------------|
| SQL Database S0 | 10 DTU | ~$15 |
| App Service S1 | 1 instance | ~$73 |
| Azure OpenAI S0 | Pay-per-use | ~$20* |
| Speech Services S0 | Pay-per-use | ~$5* |
| Translator S1 | Pay-per-use | ~$10* |

*_Costs vary based on usage_

### Cost Reduction Options

```bash
# Scale down to Basic tier for development
az appservice plan update \
  --resource-group financebuddy-rg \
  --name $PLAN_NAME \
  --sku B1

# Use serverless SQL for development
az sql db update \
  --resource-group financebuddy-rg \
  --server $SQL_SERVER_NAME \
  --name moneymentor \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --capacity 1
```

## Monitoring and Diagnostics

### Enable Application Insights
```bash
# Create Application Insights
az monitor app-insights component create \
  --resource-group financebuddy-rg \
  --app financebuddy-insights \
  --location southafricanorth \
  --application-type web

# Link to App Service
INSIGHTS_KEY=$(az monitor app-insights component show \
  --resource-group financebuddy-rg \
  --app financebuddy-insights \
  --query 'instrumentationKey' --output tsv)

az webapp config appsettings set \
  --resource-group financebuddy-rg \
  --name $API_APP_NAME \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSIGHTS_KEY
```

## Troubleshooting

### Common Deployment Issues

#### 1. Azure OpenAI Access Denied
**Error**: `The subscription is not registered to use namespace 'Microsoft.CognitiveServices/accounts/OpenAI'`

**Solution**: 
- Apply for Azure OpenAI access at https://aka.ms/oai/access
- Wait for approval (can take several days)
- Verify subscription is approved in Azure portal

#### 2. SQL Connection Issues
**Error**: Cannot connect to SQL Server

**Solutions**:
```bash
# Check firewall rules
az sql server firewall-rule list \
  --resource-group financebuddy-rg \
  --server $SQL_SERVER_NAME

# Add your IP if missing
az sql server firewall-rule create \
  --resource-group financebuddy-rg \
  --server $SQL_SERVER_NAME \
  --name ClientIP \
  --start-ip-address $CLIENT_IP \
  --end-ip-address $CLIENT_IP
```

#### 3. Resource Quota Exceeded
**Error**: Region quota exceeded

**Solutions**:
- Deploy to a different region with available quota
- Request quota increase in Azure portal
- Use different SKU tiers

### Validation Commands

```bash
# Test SQL connectivity
sqlcmd -S $SQL_SERVER_NAME.database.windows.net \
  -d moneymentor \
  -U sqladminuser \
  -P $SQL_PASSWORD \
  -Q "SELECT 1"

# Test API endpoint
curl https://$API_APP_NAME.azurewebsites.net/health

# Test OpenAI endpoint
curl -H "api-key: $OPENAI_KEY" \
  https://$OPENAI_NAME.openai.azure.com/openai/deployments?api-version=2023-05-15
```

## Cleanup

### Remove All Resources
```bash
# Delete entire resource group (irreversible!)
az group delete \
  --name financebuddy-rg \
  --yes --no-wait
```

### Selective Resource Cleanup
```bash
# Delete only expensive resources (keep data)
az webapp delete \
  --resource-group financebuddy-rg \
  --name $API_APP_NAME

az cognitiveservices account delete \
  --resource-group financebuddy-rg \
  --name $OPENAI_NAME
```

## Integration with FinanceBuddy MAUI App

### Update MAUI App Configuration

After deployment, update your MAUI app's API endpoint:

```csharp
// In FinanceBuddy/Services/ApiClient.cs
public class ApiClient
{
    private readonly HttpClient _httpClient;
    
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Update with your deployed API URL
        _httpClient.BaseAddress = new Uri("https://your-api-name.azurewebsites.net/");
    }
}
```

## Support and Maintenance

### Regular Maintenance Tasks

1. **Monitor costs** in Azure Cost Management
2. **Update SQL firewall rules** as client IPs change
3. **Rotate API keys** periodically for security
4. **Monitor performance** through Application Insights
5. **Apply security updates** to App Service runtime

### Getting Help

- **Azure Documentation**: https://docs.microsoft.com/azure/
- **Bicep Documentation**: https://docs.microsoft.com/azure/azure-resource-manager/bicep/
- **Azure OpenAI Documentation**: https://docs.microsoft.com/azure/cognitive-services/openai/
- **GitHub Issues**: Report infrastructure issues in the project repository

---

## Contributing

To modify the infrastructure:

1. **Edit `main.bicep`** with your changes
2. **Test deployment** in a development resource group
3. **Validate outputs** and functionality
4. **Submit pull request** with detailed description

---

*This infrastructure deployment supports the SA Intervarsity Hack 2025 MoneyMentor challenge, providing scalable, secure, and cost-effective Azure services for the FinanceBuddy application.*