@echo off
REM Set the environment variable for the application
set DOTNET_ENVIRONMENT=Development

REM Run the application with the specified arguments
dotnet run --project . -- update-builder-settings --file "builderSettings.json" --prefixes "fdn_"

REM Check if the previous command succeeded
if %errorlevel%==0 (
    REM Run the pac modelbuilder command if dotnet run succeeds
    pac modelbuilder build --outdirectory .\Entities\ --settingsTemplateFile builderSettings.json --environment "https://pcflab.crm3.dynamics.com/"
)

REM Pause to keep the console open after execution
pause
