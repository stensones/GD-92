namespace Stensones.GD92.Fields;

internal static class CountedAsciiStringEncoding
{
	private const int CountBitCount = 8;

	public static SevenBitAsciiString ReadCountedAscii(
		ref EncodedMessageBuffer buffer,
		int maximumEncodedLength,
		bool isCompressed)
	{
		var encodedLength = (byte)buffer.ReadUnsignedBits(CountBitCount);

		ArgumentOutOfRangeException.ThrowIfGreaterThan(encodedLength, maximumEncodedLength, nameof(buffer));

		var encodedValue = buffer.ReadBytes(encodedLength);
		ReadOnlySpan<byte> value = isCompressed
			? CompressedAscii.Decompress(encodedValue)
			: encodedValue;
		var characters = new char[value.Length];

		for (var index = 0; index < characters.Length; index++)
		{
			characters[index] = (char)value[index];
		}

		return SevenBitAsciiString.FromValue(new string(characters));
	}

	public static byte[] ToCountedWireValue(
		SevenBitAsciiString value,
		int maximumEncodedLength,
		bool isCompressed)
	{
		var encodedValue = isCompressed
			? CompressedAscii.Compress(value.ToWireValue())
			: value.ToWireValue();

		ArgumentOutOfRangeException.ThrowIfGreaterThan(encodedValue.Length, maximumEncodedLength, nameof(value));

		return [(byte)encodedValue.Length, .. encodedValue];
	}
}
