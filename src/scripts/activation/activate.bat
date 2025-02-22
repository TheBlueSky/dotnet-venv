@echo off

REM Set the deactivate script
set "DEACTIVATE_SCRIPT=%~dp0\deactivate.bat"

REM Reset the environment if it's already activated
call "%DEACTIVATE_SCRIPT%"

REM Store the old PATH to restore it later
set "DOTNET_VIRTUAL_ENV_OLD_PATH=%PATH%"

REM Get the script's directory (environment root)
set "DOTNET_VIRTUAL_ENV_SCRIPT_DIR=%~dp0"

REM Remove trailing backslash
set "DOTNET_VIRTUAL_ENV_SCRIPT_DIR=%DOTNET_VIRTUAL_ENV_SCRIPT_DIR:~0,-1%"

REM Store the old prompt to restore it later
set "DOTNET_VIRTUAL_ENV_OLD_PROMPT=%PROMPT%"

REM Add environment to PATH
set "PATH=%DOTNET_VIRTUAL_ENV_SCRIPT_DIR%;%PATH%"

REM Set the prompt prefix environment variable
set "DOTNET_VIRTUAL_ENV_DIR_NAME=%%~nxF"

REM Change prompt to show we're in a custom environment
for %%F in (%DOTNET_VIRTUAL_ENV_SCRIPT_DIR%) do set "PROMPT=(%DOTNET_VIRTUAL_ENV_DIR_NAME%) %DOTNET_VIRTUAL_ENV_OLD_PROMPT%"

REM Set the DOTNET_VIRTUAL_ENV variable to the script's directory
set DOTNET_VIRTUAL_ENV=%DOTNET_VIRTUAL_ENV_SCRIPT_DIR%

doskey deactivate="%DEACTIVATE_SCRIPT%"
