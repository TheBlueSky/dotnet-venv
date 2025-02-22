namespace TheBlueSky.DotNet.Tools.VirtualEnvironment;

internal sealed class CommandLineArgumentsParser
{
	private const string OptionStartChar = "-";

	private static readonly Dictionary<string, string> ArgumentAliases =
		new()
		{
			{ "-?", "--help" },
			{ "-h", "--help" },
			{ "-n", "--name" },
			{ "-r", "--release" },
			{ "-v", "--verbose" },
		};

	public static Options Parse(string[] args)
	{
		var options = new Options();

		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];

			// Resolve aliases to their full form
			if (ArgumentAliases.TryGetValue(arg, out var fullArg))
			{
				arg = fullArg;
			}

			switch (arg)
			{
				case "--help":
					options.Help = true;
					break;

				case "--name":
					options.Name = i + 1 < args.Length && !args[i + 1].StartsWith(OptionStartChar) ?
						args[++i] :
						throw new ArgumentException($"Name option requires a value.");
					break;

				case "--no-logo":
					options.IsNoLogo = true;
					break;

				case "--release":
					options.Release = i + 1 < args.Length && !args[i + 1].StartsWith(OptionStartChar) ?
						args[++i] :
						throw new ArgumentException("Release option requires a value.");
					break;

				case "--verbose":
					options.IsVerbose = true;
					break;

				case "--version":
					options.IsVersion = true;
					break;

				default:
					throw new ArgumentException($"Unknown option: {arg}");
			}
		}

		return options;
	}

	public sealed class Options
	{
		public bool Help { get; set; }

		public string Name { get; set; } = Constants.VirtualEnvironmentDefaultName;

		public string Release { get; set; } = Constants.VirtualEnvironmentDefaultRelease;

		public bool IsVerbose { get; set; }

		public bool IsNoLogo { get; set; }

		public bool IsVersion { get; set; }
	}
}
