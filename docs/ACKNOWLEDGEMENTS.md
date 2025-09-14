# Acknowledgements

This document acknowledges all third-party libraries, frameworks, APIs, and resources used in the FinanceBuddy project.

## Third-Party Libraries and Packages

### .NET MAUI Framework
- **Microsoft.Maui.Controls** (Version: Latest)
  - License: MIT License
  - Purpose: Cross-platform UI framework for building native mobile and desktop applications
  - Source: https://github.com/dotnet/maui

### Community Toolkit
- **CommunityToolkit.Maui** (Version: 11.2.0)
  - License: MIT License
  - Purpose: MAUI community extensions providing alerts, behaviors, converters, and utilities
  - Source: https://github.com/CommunityToolkit/Maui
  - Used for: Snackbar alerts, platform-specific features, and enhanced UI components

### Microsoft Extensions
- **Microsoft.Extensions.Logging.Debug** (Version: 9.0.5)
  - License: MIT License
  - Purpose: Debug logging provider for development and debugging
  - Source: https://github.com/dotnet/extensions

- **Microsoft.Extensions.Http** (Version: 9.0.0)
  - License: MIT License
  - Purpose: HTTP client factory and configuration
  - Source: https://github.com/dotnet/extensions
  - Used for: API communication and HTTP client management

### Azure AI Services
- **Azure.AI.OpenAI** (Version: 2.0.0-beta.2)
  - License: MIT License
  - Purpose: Azure OpenAI service integration for AI-powered financial advice
  - Source: https://github.com/Azure/azure-sdk-for-net
  - Used for: ChatGPT integration in Money Mentor chat functionality

### Speech Recognition
- **Microsoft.CognitiveServices.Speech** (Version: 1.40.0)
  - License: Microsoft Software License
  - Purpose: Speech-to-text and text-to-speech capabilities
  - Source: https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/
  - Used for: Voice input for expense entry and accessibility features

### Entity Framework
- **Microsoft.EntityFrameworkCore** (Version: 8.0.8)
  - License: MIT License
  - Purpose: Object-relational mapping (ORM) framework
  - Source: https://github.com/dotnet/efcore

- **Microsoft.EntityFrameworkCore.InMemory** (Version: 8.0.8)
  - License: MIT License
  - Purpose: In-memory database provider for testing and development
  - Source: https://github.com/dotnet/efcore

- **Microsoft.EntityFrameworkCore.SqlServer** (Version: 8.0.8)
  - License: MIT License
  - Purpose: SQL Server database provider for production data storage
  - Source: https://github.com/dotnet/efcore

- **Microsoft.EntityFrameworkCore.Design** (Version: 8.0.8)
  - License: MIT License
  - Purpose: Design-time tools for Entity Framework migrations
  - Source: https://github.com/dotnet/efcore

### ASP.NET Core
- **Microsoft.AspNetCore.SignalR** (Version: 1.1.0)
  - License: Apache License 2.0
  - Purpose: Real-time web functionality for live updates
  - Source: https://github.com/SignalR/SignalR

- **Swashbuckle.AspNetCore** (Version: 6.6.2)
  - License: MIT License
  - Purpose: Swagger/OpenAPI documentation generation
  - Source: https://github.com/domaindrivendev/Swashbuckle.AspNetCore

## Visual Assets and Icons

### Plant Illustrations
- Custom SVG plant illustrations created specifically for the gamification system
- Stages: Seed, Sprout, Seedling, Young Plant, Mature Plant, Blooming Tree
- **License**: Custom created assets (All rights reserved to project team)
- **Purpose**: Visual representation of user's financial wellness progress

### Application Icons
- **App Icon**: Custom designed financial buddy mascot
- **Source**: Original design by project team
- **License**: All rights reserved to project team

## Fonts and Typography
- **OpenSans-Regular.ttf** and **OpenSans-Semibold.ttf**
  - License: Apache License 2.0
  - Source: Google Fonts
  - Purpose: Primary typography for the application

## Development Tools and Services

### Microsoft Visual Studio
- **IDE**: Visual Studio 2022 Community/Professional
- **License**: Microsoft Software License
- **Purpose**: Primary development environment

### Azure Services
- **Azure OpenAI Service**
  - **License**: Microsoft Azure Service Agreement
  - **Purpose**: AI-powered financial advice and chat functionality
  - **Note**: API key required for production deployment

### GitHub
- **Repository Hosting**: GitHub.com
- **License**: GitHub Terms of Service
- **Purpose**: Version control and collaboration
- **Repository**: https://github.com/DrVanHelsing/SAIntervarsityHack2025-MoneyMentor

## Frameworks and Runtime

### .NET Framework
- **.NET 9.0** (FinanceBuddy MAUI App)
  - License: MIT License
  - Source: https://github.com/dotnet/core

- **.NET 8.0** (API and Shared projects)
  - License: MIT License
  - Source: https://github.com/dotnet/core

## Architecture Patterns and Concepts

### MVVM Pattern
- **Source**: Microsoft documentation and community best practices
- **Purpose**: Separation of concerns in UI development

### Dependency Injection
- **Microsoft.Extensions.DependencyInjection**
- **License**: MIT License
- **Purpose**: Service container and dependency management

## Financial Education Content

### Currency Formatting
- **South African Rand (ZAR)** as default currency
- **Source**: .NET globalization libraries
- **License**: MIT License

## Special Acknowledgements

### AI Integration
- Thanks to Microsoft and OpenAI for providing accessible AI services
- Azure OpenAI Service enables intelligent financial advice and natural language processing

### Community
- Thanks to the .NET MAUI community for extensive documentation and examples
- Stack Overflow community for troubleshooting and best practices
- Microsoft Learn documentation for comprehensive guides

### Educational Resources
- Financial literacy concepts adapted from public financial education resources
- Gamification principles inspired by behavioral psychology research

## Compliance and Attribution

### Open Source Licenses
All open source components are used in compliance with their respective licenses:
- MIT Licensed components: Full attribution maintained
- Apache Licensed components: License notice preserved
- Microsoft Licensed components: Used within terms of service

### Data Privacy
- No personal financial data is transmitted to third parties without user consent
- Local data storage follows platform security guidelines
- AI interactions are processed through secure Azure endpoints

### Accessibility
- Built with accessibility guidelines following Microsoft's inclusive design principles
- Screen reader compatibility through semantic XAML markup
- Voice input support for users with mobility limitations

---

*Last Updated: January 2025*

*For questions about licenses or attributions, please contact the development team.*