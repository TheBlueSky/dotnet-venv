using System.Text.Json.Serialization;

namespace TheBlueSky.DotNet.Tools.VirtualEnvironment;

internal sealed record class Sdk(string Version);

internal sealed record class Releases([property: JsonPropertyName("releases-index")] IReadOnlyList<Release> ReleasesIndex);

internal sealed record class Release(
	[property: JsonPropertyName("channel-version")] string ChannelVersion,
	[property: JsonPropertyName("latest-release")] string LatestRelease,
	[property: JsonPropertyName("latest-sdk")] string LatestSdk,
	[property: JsonPropertyName("release-type")] string ReleaseType,
	[property: JsonPropertyName("support-phase")] string SupportPhase)
{
	private bool IsPreview => SupportPhase.Equals("preview", StringComparison.OrdinalIgnoreCase) || SupportPhase.Equals("go-live", StringComparison.OrdinalIgnoreCase);

	public bool IsMatching(string release) =>
		ChannelVersion.Equals(release, StringComparison.OrdinalIgnoreCase) ||
		LatestSdk.Equals(release, StringComparison.OrdinalIgnoreCase) ||
		(!IsPreview && ReleaseType.Equals(release, StringComparison.OrdinalIgnoreCase)) ||
		(IsPreview && release.Equals("preview", StringComparison.OrdinalIgnoreCase));
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Releases))]
[JsonSerializable(typeof(Release))]
internal sealed partial class ReleasesJsonContext : JsonSerializerContext;

internal sealed record class GlobalJson([property: JsonPropertyName("sdk")] GlobalJsonSdk Sdk);

internal sealed record class GlobalJsonSdk(
	[property: JsonPropertyName("version")] string Version,
	[property: JsonPropertyName("rollForward")] string RollForward = "latestPatch",
	[property: JsonPropertyName("allowPrerelease")] bool AllowPrerelease = false);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GlobalJson))]
[JsonSerializable(typeof(GlobalJsonSdk))]
internal sealed partial class GlobalJsonJsonContext : JsonSerializerContext;
