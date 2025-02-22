using TheBlueSky.DotNet.Tools.VirtualEnvironment;

var virtualEnvironmentCreator = new VirtualEnvironmentCreator(Console.WriteLine);

try
{
	var options = CommandLineArgumentsParser.Parse(args);

	if (options.IsVersion)
	{
		PrintHeader(false);
		Environment.Exit(0);
	}

	if (options.Help)
	{
		PrintHelp();
		Environment.Exit(0);
	}

	if (!string.IsNullOrWhiteSpace(options.Name))
	{
		virtualEnvironmentCreator.WithName(options.Name);
	}

	if (!string.IsNullOrWhiteSpace(options.Release))
	{
		virtualEnvironmentCreator.WithRelease(options.Release);
	}

	if (options.IsVerbose)
	{
		virtualEnvironmentCreator.WithVerboseOutput();
	}

	PrintHeader(options.IsNoLogo);

	var exitCode = await virtualEnvironmentCreator.Create();

	Environment.Exit(exitCode);
}
catch (Exception exception)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.Error.WriteLine($"Error: {exception.Message}");
	Console.ResetColor();
	Console.WriteLine();

	PrintHelp();

	Environment.Exit(2);
}

static void PrintHeader(bool isNoLogo)
{
	if (isNoLogo)
	{
		return;
	}

	Console.Write(Constants.ApplicationName);
	Console.ForegroundColor = ConsoleColor.Cyan;
	Console.WriteLine($" {Constants.Version.ToString(2)}");
	Console.ResetColor();
}

static void PrintHelp()
{
	PrintHeader(false);

	Console.WriteLine();
	Console.WriteLine($"USAGE:");
	Console.WriteLine($"    dotnet-venv [OPTIONS]");
	Console.WriteLine();
	Console.WriteLine($"OPTIONS:");
	Console.WriteLine($"    -h, --help                 Print help information.");
	Console.WriteLine($"    -n, --name <ENV_DIR>       The directory to create the virtual environment in. The default is a directory named {Constants.VirtualEnvironmentDefaultName} inside the current directory.");
	Console.WriteLine($"        --no-logo              Suppress the application logo.");
	Console.WriteLine($"    -r, --release <RELEASE>    The .NET SDK release to install. Can be STS, LTS, or Preview, or a 2-part or 3-part version, such as 7.0, 8.0.404, or 9.0.0-preview.7.24405.7. The default is {Constants.VirtualEnvironmentDefaultRelease}.");
	Console.WriteLine($"    -v, --verbose              Enable verbose output.");
	Console.WriteLine($"        --version              Show the application version and exit.");
	Console.WriteLine();
}
