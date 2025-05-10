# Dataverse.Earlybound.Console

## Overview
The `Dataverse.Earlybound.Console` project is a .NET console application designed to interact with Microsoft Dataverse. This project includes functionality for updating builder settings and generating early-bound classes for Dataverse entities.

## Prerequisites
- .NET SDK installed on your machine.
- Access to Azure Service Principal credentials with dataverse permissions for your environment.
- A Dataverse environment URL.

## Configuring Azure Service Principal
To authenticate with Dataverse, you need to configure an Azure Service Principal. This can be done securely using the `dotnet user-secrets` tool.

### Steps to Configure Azure Service Principal

1. **Set Azure Service Principal Credentials**
   Use the following commands to set the required credentials. These credentials will override the placeholders in `appsettings.Production.json`:
   ```bash
   dotnet user-secrets set "Dataverse:ClientId" "<your-client-id>"
   dotnet user-secrets set "Dataverse:ClientSecret" "<your-client-secret>"
   dotnet user-secrets set "Dataverse:Url" "<your-dataverse-url>"
   ```
   Replace `<your-client-id>`, `<your-client-secret>`, and `<your-dataverse-url>` with the values from your Azure App registration and Dataverse environment.


## Running the Application

> *NOTE*: Edit the `run-update-builder-settings.bat` file so that the output directory fits your need.

1. Open a terminal and navigate to the `Dataverse.Earlybound.Console` directory.
2. Run the application using the provided batch file:
   ```bash
   run-update-builder-settings.bat
   ```
   This will execute the application with the necessary parameters and environment variables.

## Additional Notes
- Ensure that the `builderSettings.json` file is correctly configured before running the application.
- The `DOTNET_ENVIRONMENT` is set to `Development` by default in the batch file.

For more information, refer to the official [Microsoft Dataverse documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/).
