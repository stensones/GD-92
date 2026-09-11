namespace Stensones.GD92.Fields;

internal static class MobiliseMessageStringEncoding
{
	public static SevenBitAsciiString ReadCountedAscii(
		ref EncodedMessageBuffer buffer,
		int maximumEncodedLength,
		bool isCompressed)
	{
		var encodedLength = (byte)buffer.ReadUnsignedBits(8);

		ArgumentOutOfRangeException.ThrowIfGreaterThan(encodedLength, maximumEncodedLength, nameof(buffer));

		var encodedValue = new byte[encodedLength];

		for (var index = 0; index < encodedValue.Length; index++)
		{
			encodedValue[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		var value = isCompressed
			? CompressedAscii.Decompress(encodedValue)
			: encodedValue;

		return SevenBitAsciiString.FromValue(new string(value.Select(character => (char)character).ToArray()));
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
