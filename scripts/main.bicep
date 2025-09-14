// MoneyMentor infrastructure - South Africa context
// Deploy at resource group scope: az deployment group create -g <rg> -f infra/bicep/main.bicep -p sqlAdminLogin=... sqlAdminPassword=... 

param projectName string = 'moneymentor'
param location string = 'southafricanorth'

// Azure OpenAI is not available in South Africa regions yet; choose closest approved region
@description('Azure OpenAI deployment region (e.g., westeurope, swedencentral, eastus). Subscription must be approved.')
param openAIlocation string = 'westeurope'

@secure()
@description('SQL admin login password (min complexity).')
param sqlAdminPassword string

@minLength(1)
@description('SQL admin login username.')
param sqlAdminLogin string = 'sqladminuser'

@description('Add firewall rule for the current client IP')
param addClientIP bool = true

@description('Client IPv4 address to allow. Ignored if addClientIP=false.')
param clientIp string = '0.0.0.0'

var nameSuffix = uniqueString(resourceGroup().id)
var baseName = toLower('${projectName}${substring(nameSuffix,0,6)}')
var dbName = 'moneymentor'

// ------------------- SQL -------------------
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${baseName}-sql'
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    publicNetworkAccess: 'Enabled'
    minimalTlsVersion: '1.2'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: '${sqlServer.name}/${dbName}'
  location: location
  sku: {
    name: 'S0'
    tier: 'Standard'
    capacity: 10
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource fwAzureServices 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: '${sqlServer.name}/AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource fwClient 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = if (addClientIP) {
  name: '${sqlServer.name}/ClientIP'
  properties: {
    startIpAddress: clientIp
    endIpAddress: clientIp
  }
}

// ------------------- Cognitive Services -------------------
// Speech (South Africa North)
resource speech 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: '${baseName}-speech'
  location: location
  kind: 'SpeechServices'
  sku: { name: 'S0' }
  properties: {
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

// Translator (South Africa North)
resource translator 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: '${baseName}-translator'
  location: location
  kind: 'TextTranslation'
  sku: { name: 'S1' }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

// Azure OpenAI (closest region)
resource openai 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: '${baseName}-openai'
  location: openAIlocation
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

// ------------------- App Service (API) -------------------
var sqlConnectionString = 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${dbName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
var openAIEndpoint = 'https://${openai.name}.openai.azure.com/'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${baseName}-plan'
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
    size: 'S1'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${baseName}-api'
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'WEBSITE_RUN_FROM_PACKAGE', value: '0' }
        // Connection string (ADO.NET) for API
        { name: 'ConnectionStrings__DefaultConnection', value: sqlConnectionString }
        // MoneyMentor config placeholders
        { name: 'AzureOpenAI__Endpoint', value: openAIEndpoint }
        { name: 'AzureOpenAI__DeploymentName', value: 'gpt-4o' }
        // Set the key via a slot setting after deploy (do not output here)
        { name: 'AzureOpenAI__ApiKey', value: '' }
      ]
    }
  }
}

// Optional: Azure SignalR Service for scale-out (disabled by default)
@description('Provision Azure SignalR Service')
param createSignalR bool = false

resource signalr 'Microsoft.SignalRService/SignalR@2023-02-01' = if (createSignalR) {
  name: '${baseName}-signalr'
  location: location
  sku: {
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
  }
}

// ------------------- Outputs -------------------
output apiUrl string = 'https://${apiApp.name}.azurewebsites.net'
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = dbName
output sqlConnection string = sqlConnectionString
output speechRegion string = location
output speechKeyEndpoint string = speech.properties.endpoint
output translatorEndpoint string = translator.properties.endpoint
output openAIEndpointOut string = openAIEndpoint
output openAIResourceId string = openai.id
