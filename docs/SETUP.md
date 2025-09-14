# Setup Guide for FinanceBuddy

This document provides comprehensive instructions for setting up, configuring, and running the FinanceBuddy application in your development environment.

## Prerequisites

### System Requirements

#### Windows Development
- **Windows 10 version 1903 or higher** (Windows 11 recommended)
- **Visual Studio 2022** (version 17.8 or later)
- **Windows Subsystem for Android** (for Android emulation)
- **At least 8GB RAM** (16GB recommended for optimal performance)
- **20GB free disk space** for SDKs and emulators

#### macOS Development  
- **macOS 12.0 Monterey or higher** (macOS 13 Ventura recommended)
- **Xcode 14 or later** (for iOS development)
- **Visual Studio 2022 for Mac** or **Visual Studio Code** with C# extension
- **Android Studio** (for Android development on macOS)

#### Cross-Platform Development
- **.NET 9.0 SDK** (required for FinanceBuddy MAUI app)
- **.NET 8.0 SDK** (required for API and Shared projects)
- **Git** for version control

## Installation Steps

### 1. Install .NET SDKs

#### Download and Install .NET 9.0
```powershell
# Windows (PowerShell as Administrator)
winget install Microsoft.DotNet.SDK.9

# Alternative: Download from https://dotnet.microsoft.com/download/dotnet/9.0
```

#### Download and Install .NET 8.0
```powershell
# Windows (PowerShell as Administrator)
winget install Microsoft.DotNet.SDK.8

# Alternative: Download from https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Verify Installation
```bash
dotnet --list-sdks
# Should show both 8.0.x and 9.0.x versions
```

### 2. Install Visual Studio 2022

#### Windows Installation
1. Download **Visual Studio 2022 Community** (free) or Professional/Enterprise
2. During installation, ensure these workloads are selected:
   - **.NET Multi-platform App UI development**
   - **ASP.NET and web development**
   - **Azure development** (optional, for deployment)

#### Required Individual Components
Ensure these are selected in the installer:
- **.NET MAUI**
- **Android SDK Platform 34**
- **Android SDK Build-Tools**
- **Android Emulator**
- **Intel Hardware Accelerated Execution Manager (HAXM)** (Intel CPUs)
- **Windows App SDK**

### 3. Configure Android Development

#### Android SDK Setup
```bash
# Set ANDROID_HOME environment variable
# Windows: Add to System Environment Variables
ANDROID_HOME=C:\Users\[USERNAME]\AppData\Local\Android\Sdk

# Add to PATH
PATH=%PATH%;%ANDROID_HOME%\platform-tools;%ANDROID_HOME%\tools
```

#### Create Android Virtual Device (AVD)
1. Open **Android Device Manager** in Visual Studio
2. Create new device with:
   - **API Level 34** (Android 14)
   - **System Image**: Google APIs (x86_64)
   - **RAM**: 4GB or higher
   - **Internal Storage**: 8GB or higher

### 4. Configure iOS Development (macOS only)

#### Xcode Setup
```bash
# Install Xcode from Mac App Store
# Accept Xcode license
sudo xcodebuild -license accept

# Install iOS Simulator
xcode-select --install
```

#### iOS Simulator Configuration
- Open Xcode → Window → Devices and Simulators
- Add iOS simulator devices for testing (iPhone 15, iPad, etc.)

## Project Setup

### 1. Clone the Repository

```bash
# Clone the project
git clone https://github.com/DrVanHelsing/SAIntervarsityHack2025-MoneyMentor.git

# Navigate to project directory
cd SAIntervarsityHack2025-MoneyMentor
```

### 2. Restore NuGet Packages

```bash
# Restore all project dependencies
dotnet restore

# Alternative: Open solution in Visual Studio and restore packages automatically
```

### 3. Configure Azure OpenAI Service (Required for AI Features)

#### Create Azure OpenAI Resource
1. **Create Azure Account** (if you don't have one)
   - Go to https://portal.azure.com
   - Sign up for free account (includes free credits)

2. **Create OpenAI Resource**
   - Search for "OpenAI" in Azure Portal
   - Click "Create" → "Azure OpenAI"
   - Choose resource group and region
   - Select pricing tier (Standard recommended)

3. **Deploy GPT Model**
   - Go to Azure OpenAI Studio
   - Navigate to "Deployments"
   - Create new deployment with:
     - Model: **gpt-3.5-turbo** or **gpt-4**
     - Deployment name: **money-mentor-chat**

#### Configure API Keys
Create `appsettings.Development.json` in the API project:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "money-mentor-chat",
    "ApiVersion": "2024-02-15-preview"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**⚠️ Important:** Never commit API keys to version control. Use environment variables in production.

### 4. Database Configuration

The project uses **Entity Framework Core** with multiple database providers:

#### Development (In-Memory Database)
- No additional setup required
- Data is reset on each application restart
- Perfect for development and testing

#### Production (SQL Server)
```bash
# Install SQL Server LocalDB (Windows)
# Or use Azure SQL Database for cloud deployment

# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MoneyMentorDB;Trusted_Connection=true"
}
```

#### Apply Database Migrations
```bash
# Navigate to API project
cd MoneyMentor.ApiOrchestrator

# Create and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Build and Run the Application

#### Build All Projects
```bash
# From solution root directory
dotnet build
```

#### Run the API Server
```bash
# Terminal 1: Start the API server
cd MoneyMentor.ApiOrchestrator
dotnet run

# API will be available at:
# - https://localhost:7001 (HTTPS)
# - http://localhost:5001 (HTTP)
# - Swagger UI: https://localhost:7001/swagger
```

#### Run the MAUI Application
```bash
# Terminal 2: Start the MAUI app
cd FinanceBuddy

# For Android
dotnet build -t:Run -f net9.0-android

# For Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0

# For iOS (macOS only)
dotnet build -t:Run -f net9.0-ios

# For macOS (macOS only)
dotnet build -t:Run -f net9.0-maccatalyst
```

#### Alternative: Use Visual Studio
1. Open `SAIntervarsityHack2025-MoneyMentor.sln` in Visual Studio
2. Set **Multiple Startup Projects**:
   - MoneyMentor.ApiOrchestrator: **Start**
   - FinanceBuddy: **Start**
3. Select target platform (Android Emulator, Windows Machine, etc.)
4. Press **F5** to run

## Configuration Options

### Environment Variables

#### Development Environment
```bash
# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:AZURE_OPENAI_ENDPOINT = "your-endpoint"
$env:AZURE_OPENAI_API_KEY = "your-api-key"

# macOS/Linux (bash)
export ASPNETCORE_ENVIRONMENT=Development
export AZURE_OPENAI_ENDPOINT=your-endpoint
export AZURE_OPENAI_API_KEY=your-api-key
```

#### Production Environment
```bash
# Set these in your hosting environment
ASPNETCORE_ENVIRONMENT=Production
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-production-api-key
CONNECTION_STRING=your-production-database-connection-string
```

### Application Settings

#### API Configuration (`MoneyMentor.ApiOrchestrator/appsettings.json`)
```json
{
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:7001",
      "http://localhost:5001"
    ]
  }
}
```

#### MAUI App Configuration
- Default API endpoint: `https://localhost:7001`
- Default currency: South African Rand (ZAR)
- Gamification features: Enabled by default
- Speech recognition: Optional (requires microphone permissions)

## Troubleshooting

### Common Issues and Solutions

#### 1. Build Errors

**Problem**: Missing .NET SDKs
```bash
# Solution: Verify SDK installation
dotnet --list-sdks
# Reinstall missing SDKs if needed
```

**Problem**: NuGet package restore failures
```bash
# Solution: Clear NuGet cache and restore
dotnet nuget locals all --clear
dotnet restore
```

#### 2. Android Emulator Issues

**Problem**: Emulator won't start
```bash
# Solution: Enable hardware acceleration
# Intel: Install HAXM
# AMD: Enable Windows Hypervisor Platform
```

**Problem**: App deployment fails
```bash
# Solution: Restart ADB and emulator
adb kill-server
adb start-server
# Restart emulator
```

#### 3. API Connection Issues

**Problem**: MAUI app can't connect to API
- **Ensure API is running** on correct port (7001/5001)
- **Check firewall settings** - allow Visual Studio and dotnet.exe
- **Update API endpoint** in MAUI app if using different ports

**Problem**: Azure OpenAI API errors
- **Verify API key** is correct and not expired
- **Check endpoint URL** format
- **Ensure model deployment** is active in Azure OpenAI Studio
- **Check quota limits** in Azure portal

#### 4. Platform-Specific Issues

**Windows**: 
- Enable **Developer Mode** in Windows Settings
- Install **Windows App Runtime** if missing

**macOS**:
- Sign code with **Apple Developer certificate** for device deployment
- Enable **Developer Options** on iOS devices

**Android**:
- Enable **USB Debugging** on physical devices
- Install appropriate **Android SDK platforms**

### Development Tips

#### Hot Reload
- **MAUI Hot Reload**: Enabled by default in debug mode
- **API Hot Reload**: Use `dotnet watch run` in API project
- Changes to XAML and C# code reload automatically

#### Debugging
- Set breakpoints in Visual Studio
- Use **MAUI Blazor Hybrid** debugging tools
- Monitor API calls with **Swagger UI**
- Use **Azure OpenAI Studio** playground for testing AI responses

#### Performance
- Use **Release mode** for performance testing
- Enable **AOT compilation** for production builds
- Profile with **Visual Studio Diagnostic Tools**

### Getting Help

#### Documentation
- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Azure OpenAI Documentation](https://docs.microsoft.com/azure/cognitive-services/openai/)

#### Community Support
- [.NET MAUI GitHub Discussions](https://github.com/dotnet/maui/discussions)
- [Stack Overflow - .NET MAUI](https://stackoverflow.com/questions/tagged/maui)
- [Microsoft Developer Community](https://developercommunity.visualstudio.com/)

#### Project-Specific Support
- Check existing [GitHub Issues](https://github.com/DrVanHelsing/SAIntervarsityHack2025-MoneyMentor/issues)
- Create new issue with detailed error information
- Contact development team through repository

## Next Steps

Once setup is complete:

1. **Explore the Application**: Test all features including expense tracking, AI chat, and plant gamification
2. **Review Documentation**: Read `USAGE.md` for detailed feature explanations
3. **Check Team Information**: See `TEAM.md` for development team contacts

---

*Setup complete! Your FinanceBuddy development environment is ready for building amazing financial wellness experiences! 🚀*