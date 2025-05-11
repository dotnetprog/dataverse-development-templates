# Blueprint.Plugins

Blueprint.Plugins is a .NET project designed to provide a framework for developing plugins for the Dataverse platform. This repository contains the core plugin logic, business rules, and supporting services to streamline plugin development.

## Project Structure

The project is organized into the following main directories:

- **Common/**: Contains shared utilities and helper classes, such as `EntityHelper.cs`.
- **Features/**: Contains feature-specific plugins and business rules. For example:
  - `CustomersManagement/ContactEventsPlugin.cs`: A plugin for managing contact events.
  - `CustomersManagement/BusinessRules/EnsureContactEmailAddressIsNotEmptyBusinessRule.cs`: A business rule to ensure contact email addresses are not empty.
- **Platform/**: Contains platform-level abstractions and utilities, such as `ExecutionMode.cs`, `IBusinessRule.cs`, and `Message.cs`.
- **Services/**: Contains services for interacting with environment variables and other external dependencies, such as `CachedEnvironmentVariableService.cs` and `CrmEnvironmentVariableService.cs`.

## Prerequisites

- .NET Framework 4.6.2 or later.
- Visual Studio or any other IDE that supports .NET development.
- Access to a Dataverse environment for testing and deployment.

## Build and Run

### Build
To build the project, you can use the following VS Code task:

```bash
# Run this task in the terminal
Task: build
```

Alternatively, you can run the following command in the terminal:

```powershell
dotnet build plugins.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
```

### Watch
To run the project in watch mode, use the following VS Code task:

```bash
# Run this task in the terminal
Task: watch
```

Or execute the following command:

```powershell
dotnet watch run plugins.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
```

## Deployment

1. Build the project to generate the plugin assembly (`Blueprint.Plugins.dll`).
2. Use the `pac plugin push` command to deploy the plugin to your Dataverse environment. Below is an example command:

```powershell
pac plugin push --pluginId <PLUGIN_ID> --pluginFile "bin\Debug\net462\Blueprint.Plugins.dll" --environment <ENVIRONMENT_URL> --type Assembly
```

### Parameters:
- `<PLUGIN_ID>`: The ID of the plugin assembly or package.
- `<ENVIRONMENT_URL>`: The URL of the target Dataverse environment.

Refer to the [Microsoft documentation](https://learn.microsoft.com/en-us/power-platform/developer/cli/reference/plugin#pac-plugin-push) for more details on the `pac plugin push` command.

### Alternative: Deploy as a NuGet Package

1. Build the project to generate the NuGet package (`plugins.1.0.0.nupkg`) located in the `bin\Debug` directory.
2. Use the `pac plugin push` command to deploy the NuGet package to your Dataverse environment. Below is an example command:

```powershell
pac plugin push --pluginFile "bin\Debug\plugins.1.0.0.nupkg" --environment <ENVIRONMENT_URL>
```

### Parameters:
- `<ENVIRONMENT_URL>`: The URL of the target Dataverse environment.

Refer to the [Microsoft documentation](https://learn.microsoft.com/en-us/power-platform/developer/cli/reference/plugin#pac-plugin-push) for more details on deploying plugins as NuGet packages.

## Additional Guidelines

For more details on how this project adheres to vertical slicing architecture and SOLID principles, refer to the [Guidelines](./Guideline.md).

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Commit your changes and push the branch.
4. Submit a pull request.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact

For questions or support, please contact the repository maintainers or open an issue in the GitHub repository.
