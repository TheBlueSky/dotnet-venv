using System.Reflection;

namespace TheBlueSky.DotNet.Tools.VirtualEnvironment;

internal static class Constants
{
	public const string ApplicationName = ".NET Virtual Environment";
	public const string VirtualEnvironmentDefaultName = ".net";
	public const string VirtualEnvironmentDefaultRelease = "LTS";

	public static readonly Assembly ApplicationAssembly = typeof(VirtualEnvironmentCreator).Assembly;
	public static readonly Version Version = ApplicationAssembly.GetName().Version!;
}
