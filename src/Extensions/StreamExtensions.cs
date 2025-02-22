using System.Text;

namespace TheBlueSky.DotNet.Tools.VirtualEnvironment.Extensions;

internal static class StreamExtensions
{
	public static void ReplacePlaceholders(
		this Stream inputStream,
		Stream outputStream,
		IDictionary<string, string> replacements,
		int bufferSize = 4096)
	{
		var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false); // UTF-8 without BOM

		using var reader = new StreamReader(inputStream, encoding, false, bufferSize, true);
		using var writer = new StreamWriter(outputStream, encoding, bufferSize, true);

		var outputBuffer = new StringBuilder(bufferSize);
		var keyBuffer = new StringBuilder();
		var state = 0; // 0: normal, 1: '{' found, 2: in placeholder, 3: '}' found
		var buffer = new char[bufferSize];
		int bytesRead;

		while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
		{
			for (var i = 0; i < bytesRead; i++)
			{
				var current = buffer[i];

				switch (state)
				{
					case 0: // Normal state
						if (current == '{')
						{
							state = 1;
						}
						else
						{
							outputBuffer.Append(current);
						}

						break;

					case 1: // Previous char was '{'
						if (current == '{')
						{
							state = 2;
							keyBuffer.Clear();
						}
						else
						{
							outputBuffer.Append('{').Append(current);
							state = 0;
						}

						break;

					case 2: // Inside placeholder key
						if (current == '}')
						{
							state = 3;
						}
						else
						{
							keyBuffer.Append(current);
						}

						break;

					case 3: // Previous char was '}' in placeholder
						if (current == '}')
						{
							var key = keyBuffer.ToString();

							if (replacements.TryGetValue(key, out var replacement))
							{
								outputBuffer.Append(replacement);
							}
							else
							{
								outputBuffer.Append($"{{{{{key}}}}}"); // Not found, write back
							}

							keyBuffer.Clear();
							state = 0;
						}
						else
						{
							keyBuffer.Append('}').Append(current);
							state = 2;
						}

						break;
				}

				// Flush output buffer if it's full
				if (outputBuffer.Length >= bufferSize)
				{
					writer.Write(outputBuffer.ToString());
					outputBuffer.Clear();
				}
			}
		}

		// Handle remaining states after processing all input
		switch (state)
		{
			case 1:
				outputBuffer.Append('{');
				break;
			case 2:
				outputBuffer.Append("{{").Append(keyBuffer);
				break;
			case 3:
				outputBuffer.Append("{{").Append(keyBuffer).Append('}');
				break;
		}

		// Write any remaining output
		if (outputBuffer.Length > 0)
		{
			writer.Write(outputBuffer.ToString());
		}
	}
}
