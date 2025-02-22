using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;
using TheBlueSky.DotNet.Tools.VirtualEnvironment.Extensions;

namespace TheBlueSky.DotNet.Tools.VirtualEnvironment;

internal sealed class VirtualEnvironmentCreator
{
	private readonly VirtualEnvironmentOptions _options;
	private readonly Action<string> _printMessage;

	public VirtualEnvironmentCreator(Action<string> printMessage)
	{
		_options = new VirtualEnvironmentOptions();
		_printMessage = printMessage;
	}

	public VirtualEnvironmentCreator WithName(string name)
	{
		_options.Name = name;
		return this;
	}

	public VirtualEnvironmentCreator WithRelease(string release)
	{
		_options.Release = release;
		return this;
	}

	public VirtualEnvironmentCreator WithVerboseOutput()
	{
		_options.IsVerbose = true;
		return this;
	}

	public async Task<int> Create()
	{
		var vEnvName = _options.Name;
		var requestedRelease = _options.Release;

		PrintMessage($"Looking for installed {requestedRelease} release.", _options.IsVerbose);

		var releases = await GetDotNetReleases();
		var matchingRelease = releases.FirstOrDefault(release => release.IsMatching(requestedRelease));

		if (matchingRelease is null)
		{
			PrintMessage($"The {requestedRelease} release does not seem to be a valid .NET release.", true);
			PrintMessage("If you are not connected to the internet, it is possible that 'dotnet-venv' data is outdated.", true);
			PrintMessage("Connect to the internet and try again.", true);

			return 1;
		}

		var latestInstalledSdks = await GetInstalledDotNetSdks();

		if (latestInstalledSdks.Count == 0)
		{
			PrintMessage($"No .NET SDK is installed. The latest {matchingRelease.LatestSdk} release will be installed.", _options.IsVerbose);
		}
		else
		{
			var isMissing = latestInstalledSdks.FirstOrDefault(sdk => sdk.Version.Equals(matchingRelease.LatestSdk, StringComparison.OrdinalIgnoreCase)) is null;

			if (isMissing)
			{
				PrintMessage($"The {matchingRelease.LatestSdk} release is not installed.", _options.IsVerbose);
			}
			else
			{
				PrintMessage($"The {requestedRelease} release is already installed.", _options.IsVerbose);

				await CreateGlobalJson(matchingRelease.LatestSdk);

				return 0;
			}
		}

		Directory.CreateDirectory(vEnvName);

		await InstallDotNetSdk(matchingRelease.LatestSdk, vEnvName, PrintMessage, _options.IsVerbose);

		await CreateGlobalJson(matchingRelease.LatestSdk);

		CreateVirtualEnvironmentActivationScript(vEnvName);

		return 0;
	}

	private void PrintMessage(string message, bool isVerbose)
	{
		if (isVerbose)
		{
			_printMessage(message);
		}
	}

	private static async Task<IReadOnlyList<Release>> GetDotNetReleases()
	{
		var isInternetConnected = false;

		try
		{
			using var ping = new Ping();
			var pingReply = await ping.SendPingAsync("raw.githubusercontent.com");

			isInternetConnected = pingReply is { Status: IPStatus.Success };
		}
		catch (Exception)
		{
		}

		return isInternetConnected ?
			await GetDotNetReleasesFromInternet() :
			await GetDotNetReleasesFromEmbeddedResource();

		static async Task<IReadOnlyList<Release>> GetDotNetReleasesFromInternet()
		{
			using var httpClient = new HttpClient();
			var releasesJson = await httpClient.GetStringAsync("https://raw.githubusercontent.com/dotnet/core/refs/heads/main/release-notes/releases-index.json");
			var releases = JsonSerializer.Deserialize(releasesJson, ReleasesJsonContext.Default.Releases);
			return releases!.ReleasesIndex;
		}

		static async Task<IReadOnlyList<Release>> GetDotNetReleasesFromEmbeddedResource()
		{
			using var dotnetReleasesFileStream = Constants.ApplicationAssembly.GetManifestResourceStream("data.dotnet-releases.json")!;
			var releases = await JsonSerializer.DeserializeAsync(dotnetReleasesFileStream, ReleasesJsonContext.Default.Releases);
			return releases!.ReleasesIndex;
		}
	}

	private static async Task<IReadOnlyCollection<Sdk>> GetInstalledDotNetSdks()
	{
		try
		{
			var process = StartNewProcess("dotnet", "--list-sdks");
			var sdks = new List<Sdk>();

			while (!process.StandardOutput.EndOfStream)
			{
				var line = await process.StandardOutput.ReadLineAsync();

				if (line is null)
				{
					continue;
				}

				var lineSpan = line.AsSpan();
				var versionIndex = lineSpan.IndexOf(' ');
				var version = lineSpan[..versionIndex];
				var directory = lineSpan[(versionIndex + 1)..].TrimStart('[').TrimEnd(']');
				sdks.Add(new Sdk(version.ToString(), directory.ToString()));
			}

			await process.WaitForExitAsync();

			return sdks;
		}
		catch (Win32Exception)
		{
			return [];
		}
	}

	private static async Task CreateGlobalJson(string version)
	{
		var globalJson = new GlobalJson(new GlobalJsonSdk(version));
		await using var globalJsonFileStream = File.Create("global.json");
		await JsonSerializer.SerializeAsync(globalJsonFileStream, globalJson, GlobalJsonJsonContext.Default.GlobalJson);
	}

	private static async Task InstallDotNetSdk(string? version, string directory, Action<string, bool> printMessage, bool isVerbose)
	{
		const string installationPart = ".installation.";

		printMessage($"Installing .NET SDK {version} to '{directory}'.", isVerbose);

		var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		var fileExtension = isWindows ? "ps1" : "sh";

		var embeddedInstallationScript = Constants.ApplicationAssembly.GetManifestResourceNames()
			.Single(name => name.Contains(installationPart, StringComparison.OrdinalIgnoreCase) && name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));

		var fileName = embeddedInstallationScript[(embeddedInstallationScript.IndexOf(installationPart, StringComparison.OrdinalIgnoreCase) + installationPart.Length)..];
		var installationScript = Path.Combine(Path.GetTempPath(), Path.GetFileName(fileName));

		using var installationScriptStream = Constants.ApplicationAssembly.GetManifestResourceStream(embeddedInstallationScript)!;
		using var installationScriptFileStream = File.Create(installationScript);

		await installationScriptStream.CopyToAsync(installationScriptFileStream);

		installationScriptStream.Close();

		await installationScriptFileStream.FlushAsync();
		installationScriptFileStream.Close();

		var process = isWindows ?
			StartNewProcess("powershell", $"-ExecutionPolicy bypass -File \"{installationScript}\" -Version {version} -InstallDir {directory}") :
			StartNewProcess("bash", $"\"{installationScript}\" --version {version} --install-dir {directory}");

		while (!process.StandardOutput.EndOfStream)
		{
			var line = await process.StandardOutput.ReadLineAsync();
			printMessage(line ?? "", isVerbose);
		}

		await process.WaitForExitAsync();

		File.Delete(installationScript);
	}

	private static Process StartNewProcess(string fileName, string arguments, string workingDirectory = ".")
	{
		var process = new Process
		{
			StartInfo =
			{
				FileName = fileName,
				Arguments = arguments,
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			},
		};

		process.Start();

		return process;
	}

	private static void CreateVirtualEnvironmentActivationScript(string destinationDirectory)
	{
		const string activationPart = ".activation.";

		var activationScripts = Constants.ApplicationAssembly.GetManifestResourceNames().Where(name => name.Contains(activationPart, StringComparison.OrdinalIgnoreCase));

		foreach (var activationScript in activationScripts)
		{
			var fileName = activationScript[(activationScript.IndexOf(activationPart, StringComparison.OrdinalIgnoreCase) + activationPart.Length)..];
			var activationScriptDestination = Path.Combine(destinationDirectory, Path.GetFileName(fileName));
			using var activationScriptStream = Constants.ApplicationAssembly.GetManifestResourceStream(activationScript);
			using var activationScriptFileStream = File.Create(activationScriptDestination);

			if (fileName.Equals("activate", StringComparison.OrdinalIgnoreCase))
			{
				var replacements = new Dictionary<string, string>
				{
					["DOTNET_VIRTUAL_ENV_PATH"] = Path.GetFullPath(destinationDirectory),
					["DOTNET_VIRTUAL_ENV_DIR"] = destinationDirectory,
				};

				activationScriptStream!.ReplacePlaceholders(activationScriptFileStream, replacements);
			}
			else
			{
				activationScriptStream!.CopyTo(activationScriptFileStream);
			}
		}
	}

	private sealed record class VirtualEnvironmentOptions
	{
		public string Name { get; set; } = Constants.VirtualEnvironmentDefaultName;

		public string Release { get; set; } = Constants.VirtualEnvironmentDefaultRelease;

		public bool IsVerbose { get; set; }
	}
}
