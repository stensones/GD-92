namespace Stensones.GD92.Fields;

internal static class CompressedAscii
{
	private const byte EscapeByte = 0x1B;
	private const byte EscapedEscapeRunLength = 1;
	private const int MinimumCompressibleRunLength = 4;
	private const int EscapeSequenceLength = 3;
	private const int EscapedCharacterOffset = 1;
	private const int RunLengthOffset = 2;
	private const string IncompleteEscapeSequenceMessage = "The compressed text contains an incomplete escape sequence.";
	private const string InvalidRunLengthMessage = "The compressed text contains an invalid run length.";

	public static byte[] Compress(ReadOnlySpan<byte> value)
	{
		var compressedValue = new List<byte>();

		for (var index = 0; index < value.Length;)
		{
			var runLength = 1;

			while (index + runLength < value.Length && value[index + runLength] == value[index])
			{
				runLength++;
			}

			if (runLength >= MinimumCompressibleRunLength)
			{
				var remainingRunLength = runLength;

				while (remainingRunLength >= MinimumCompressibleRunLength)
				{
					var compressedRunLength = Math.Min(remainingRunLength, byte.MaxValue);
					compressedValue.Add(EscapeByte);
					compressedValue.Add(value[index]);
					compressedValue.Add((byte)compressedRunLength);
					remainingRunLength -= compressedRunLength;
				}

				for (var runIndex = 0; runIndex < remainingRunLength; runIndex++)
				{
					compressedValue.Add(value[index]);
				}
			}
			else if (value[index] == EscapeByte)
			{
				for (var runIndex = 0; runIndex < runLength; runIndex++)
				{
					compressedValue.Add(EscapeByte);
					compressedValue.Add(EscapeByte);
					compressedValue.Add(EscapedEscapeRunLength);
				}
			}
			else
			{
				for (var runIndex = 0; runIndex < runLength; runIndex++)
				{
					compressedValue.Add(value[index]);
				}
			}

			index += runLength;
		}

		return compressedValue.ToArray();
	}

	public static byte[] Decompress(ReadOnlySpan<byte> value)
	{
		var decompressedValue = new List<byte>();

		for (var index = 0; index < value.Length;)
		{
			if (value[index] != EscapeByte)
			{
				decompressedValue.Add(value[index]);
				index++;
				continue;
			}

			if (index + RunLengthOffset >= value.Length)
			{
				throw new InvalidOperationException(IncompleteEscapeSequenceMessage);
			}

			var character = value[index + EscapedCharacterOffset];
			var count = value[index + RunLengthOffset];

			if (character == EscapeByte && count == EscapedEscapeRunLength)
			{
				decompressedValue.Add(character);
			}
			else
			{
				if (count < MinimumCompressibleRunLength)
				{
					throw new InvalidOperationException(InvalidRunLengthMessage);
				}

				for (var occurrence = 0; occurrence < count; occurrence++)
				{
					decompressedValue.Add(character);
				}
			}

			index += EscapeSequenceLength;
		}

		return decompressedValue.ToArray();
	}
}
