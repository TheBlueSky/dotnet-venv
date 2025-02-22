@echo off

REM Restore old PATH
if defined DOTNET_VIRTUAL_ENV_OLD_PATH (
    set "PATH=%DOTNET_VIRTUAL_ENV_OLD_PATH%"
)

REM Restore old prompt
if defined DOTNET_VIRTUAL_ENV_OLD_PROMPT (
    set "PROMPT=%DOTNET_VIRTUAL_ENV_OLD_PROMPT%"
)

REM Clean up variables
set "DOTNET_VIRTUAL_ENV_OLD_PATH="
set "DOTNET_VIRTUAL_ENV_OLD_PROMPT="
set "DOTNET_VIRTUAL_ENV_DIR_NAME="
set "DOTNET_VIRTUAL_ENV_SCRIPT_DIR="
set "DOTNET_VIRTUAL_ENV="

REM Clear the deactivate doskey
doskey deactivate=
