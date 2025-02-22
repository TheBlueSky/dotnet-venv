# .NET Virtual Environment

## What is it?

.NET Virtual Environment, or `dotnet-venv` for short, is a .NET tool that installs the specified .NET SDK into the specified directory and pins the version using a `global.json` file.

If you are familiar with Python venv, `dotnet-venv` works similarly by creating an isolated environment with its own .NET SDK version.

## What purpose does it serve?

`dotnet-venv` allows you to quickly install a specific version of the .NET SDK into a directory for evaluation or testing. For example, if you want to try out a new .NET preview release without installing it machine-wide, `dotnet-venv` makes this easy.

## How to install it?

### Using .NET SDK

If you have .NET 8.0 or .NET 9.0 SDK installed, you can install `dotnet-venv` as a .NET Tool.

Run the following command:

```bash
dotnet tool install dotnet-venv --global
```

### Using the standalone executable

If you do not have .NET 8.0 or .NET 9.0 SDK installed, or if you prefer to use the standalone executable, follow the following steps:

1. Head to the [Releases](https://github.com/TheBlueSky/dotnet-venv/releases) page.
2. Download the latest release suitable for your operating system.
3. Extract the downloaded file to a directory of your choice.
4. Add the directory, which `dotnet-venv` is in, to the PATH environment variable.

## How to use it?

### CLI options

```text
USAGE:
    dotnet-venv [OPTIONS]

OPTIONS:
    -h, --help                 Print help information.
    -n, --name <ENV_DIR>       The directory to create the virtual environment in. The default is a directory named .net inside the current directory.
        --no-logo              Suppress the application logo.
    -r, --release <RELEASE>    The .NET SDK release to install. Can be STS, LTS, or Preview, or a 2-part or 3-part version, such as 8.0, 9.0.200, or 10.0.100-preview.1.25120.13. The default is LTS.
    -v, --verbose              Enable verbose output.
        --version              Show the application version and exit.
```

### Example

To install and use the latest .NET 9.0 SDK in a directory called `dotnet`:

1. Run the following command, which will install the latest .NET 9.0 SDK, if it is not already installed on the machine:

    ```powershell
    # On Windows (.NET Tool and standalone)
    dotnet-venv -n dotnet -r 9.0
    ```

    ```bash
    # On Linux and macOS (.NET Tool)
    dotnet-venv -n dotnet -r 9.0

    # On Linux and macOS (Standalone)
    ./dotnet-venv -n dotnet -r 9.0
    ```

2. Activate the virtual environment using the appropriate command for your operating system and terminal:

    ```powershell
    # On Windows (PowerShell)
    .\dotnet\activate.ps1
    ```

    ```batch
    REM On Windows (Command Prompt)
    dotnet\activate.bat
    ```

    ```bash
    # On Linux and macOS
    source ./dotnet/activate
    ```

3. Use the .NET CLI as you normally would. For example:

    ```bash
    dotnet new console -n MyFancyProject
    ```

4. To deactivate the virtual environment, run:

    ```bash
    deactivate
    ```

## How does it work?

Behind the scenes, `dotnet-venv` relies on:

- [Releases Index JSON file](https://raw.githubusercontent.com/dotnet/core/refs/heads/main/release-notes/releases-index.json) to check the latest releases.
- [dotnet-install scripts](https://learn.microsoft.com/en-gb/dotnet/core/tools/dotnet-install-script) to install the .NET SDK.

If the specified .NET SDK is already installed, `dotnet-venv` will simply create a `global.json` file to pin the .NET SDK version. Otherwise, it will install the SDK into the specified directory.

When you activate the virtual environment, it adds the installation directory to the `PATH` environment variable, for the current terminal session, and prefixes the terminal prompt with the virtual environment name.

## What are the limitations?

Activating a virtual environment relies on modifying the `PATH` environment variable for the current terminal session. If you launch your favourite IDE outside the context of this terminal session, it will not recognize the .NET SDK installed in the virtual environment.

If you are using Visual Studio Code, for example, after activating the virtual environment, start Visual Studio Code from the same terminal session with:

```bash
code .
```

## Credit

Credit goes to Python venv for the idea. Additionally, the activation scripts are heavily inspired by those from Python venv.
