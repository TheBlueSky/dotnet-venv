# Create deactivate function
function global:deactivate([switch]$NonDestructive) {
    # The prior PATH:
    if (Test-Path -Path Env:DOTNET_VIRTUAL_ENV_OLD_PATH) {
        Copy-Item -Path Env:DOTNET_VIRTUAL_ENV_OLD_PATH -Destination Env:PATH
        Remove-Item -Path Env:DOTNET_VIRTUAL_ENV_OLD_PATH
    }

    # The prior prompt:
    if (Test-Path -Path Function:DOTNET_VIRTUAL_ENV_OLD_PROMPT) {
        Copy-Item -Path Function:DOTNET_VIRTUAL_ENV_OLD_PROMPT -Destination Function:prompt
        Remove-Item -Path Function:DOTNET_VIRTUAL_ENV_OLD_PROMPT
    }

    # Just remove the DOTNET_VIRTUAL_ENV altogether:
    if (Test-Path -Path Env:DOTNET_VIRTUAL_ENV) {
        Remove-Item -Path Env:DOTNET_VIRTUAL_ENV
    }

    # Just remove the DOTNET_VIRTUAL_ENV_PROMPT_PREFIX altogether:
    if (Get-Variable -Name "DOTNET_VIRTUAL_ENV_PROMPT_PREFIX" -ErrorAction SilentlyContinue) {
        Remove-Variable -Name DOTNET_VIRTUAL_ENV_PROMPT_PREFIX -Scope Global -Force
    }

    # Leave deactivate function in the global namespace if requested:
    if (-not $NonDestructive) {
        Remove-Item -Path function:deactivate
    }
}

# Get the script's directory (environment root)
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ScriptDirectory = Get-Item -Path $ScriptPath
$VEnvDirectory = $ScriptDirectory.FullName.TrimEnd("\\/")

# Get the virtual environment's directory name
$VEnvDirectoryName = Split-Path -Path $VEnvDirectory -Leaf

# Deactivate any currently active virtual environment, but leave the deactivate function in place.
deactivate -NonDestructive

# Store the old prompt to restore it later
function global:DOTNET_VIRTUAL_ENV_OLD_PROMPT { "" }
Copy-Item -Path function:prompt -Destination function:DOTNET_VIRTUAL_ENV_OLD_PROMPT
New-Variable -Name DOTNET_VIRTUAL_ENV_PROMPT_PREFIX -Description ".NET virtual environment prompt prefix" -Scope Global -Option ReadOnly -Visibility Public -Value "($VEnvDirectoryName) "

function global:prompt {
    Write-Host -NoNewline -ForegroundColor Green $DOTNET_VIRTUAL_ENV_PROMPT_PREFIX
    DOTNET_VIRTUAL_ENV_OLD_PROMPT
}

# Set the environment variable VIRTUAL_ENV, can be used by tools to determine if they are running in a virtual environment.
$Env:DOTNET_VIRTUAL_ENV = $VEnvDirectory

# Add the venv to the PATH
Copy-Item -Path Env:PATH -Destination Env:DOTNET_VIRTUAL_ENV_OLD_PATH
$Env:PATH = "$VEnvDirectory$([System.IO.Path]::PathSeparator)$Env:PATH"
