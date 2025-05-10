# DataverseTemplate.ConsoleApp

## Overview
DataverseTemplate.ConsoleApp is a .NET console application template designed to interact with Microsoft Dataverse. It provides a structured approach to building console applications with features like dependency injection, configuration management, and command handling.

 > **_NOTE:_**  This cli template is built upon [Cocona CLI MicroFramework](https://github.com/mayuki/Cocona). 

## Features
- **Command Handling**: Built-in support for handling commands using Cocona.
- **Configuration Management**: Supports multiple configuration environments (Development, Production, etc.) with JSON-based configuration files.
- **Dataverse Integration**: Includes services and factories to interact with Microsoft Dataverse.
- **Dependency Injection**: Utilizes Microsoft.Extensions.DependencyInjection for managing dependencies.

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A service principal with acces to your dataverse environment. [How to create one](https://recursion.no/blogs/dataverse-setup-service-principal-access-for-environment/)
- Access to a Microsoft Dataverse environment (if using Dataverse features)

## Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd dataverse-development-templates/src/dotnet/cli/DataverseTemplate.ConsoleApp
```

### 2. Build the Project
```bash
dotnet build
```

### 3. Run the Application
```bash
dotnet run --project DataverseTemplate.ConsoleApp
```

### 4. Set Up Secrets locally
To securely store sensitive information like connection strings or API keys, you can use the `dotnet user-secrets` tool. Follow these steps:

```bash
# Set your service princpal client id
dotnet user-secrets set "Dataverse:ClientId" "<your-client-id>" --project DataverseTemplate.ConsoleApp

# Set your service princpal client secret
dotnet user-secrets set "Dataverse:ClientSecret" "<your-client-secret>" --project DataverseTemplate.ConsoleApp

# Set your Dataverse Url
dotnet user-secrets set "Dataverse:Url" "<your-environment-url>" --project DataverseTemplate.ConsoleApp
```

You can retrieve these secrets in your application using the `IConfiguration` interface.

## Configuration
The application uses JSON configuration files to manage settings. The following files are available:
- `appsettings.json`: Base configuration file.
- `appsettings.Development.json`: Development-specific settings.
- `appsettings.Production.json`: Production-specific settings.

## Commands
Commands are defined in the `Commands/DataverseCommands.cs` file. You can extend this file to add more commands as needed.

To create more advanced commands, Be sure to read the following [documentation](https://github.com/mayuki/Cocona).

## Services
The `Services/Dataverse` folder contains services and factories for interacting with Microsoft Dataverse. Key components include:
- `IDataverseClientFactory`: Interface for creating Dataverse clients.
- `SdkDataverseServiceFactory`: Factory for creating SDK-based Dataverse services.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## References
- [Cocona Documentation](https://github.com/mayuki/Cocona/blob/master/README.md): Learn more about Cocona, the library used for command handling in this project.