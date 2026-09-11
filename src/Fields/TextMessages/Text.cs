using System.Text;

namespace Stensones.GD92.Fields;

public sealed record Text : IGD9Field
{
	private const string AsciiEncodingName = "us-ascii";
	private const int Word8BitCount = 8;
	private const int LongCountBitCount = 16;
	private const int LongCountByteCount = 2;
	private const int MostSignificantLengthByteIndex = 0;
	private const int LeastSignificantLengthByteIndex = 1;

	private static readonly Encoding Ascii = Encoding.GetEncoding(
		AsciiEncodingName,
		EncoderFallback.ExceptionFallback,
		DecoderFallback.ExceptionFallback);

	private readonly byte[] value;

	private Text(byte[] value)
	{
		this.value = value;
	}

	public string Value => Ascii.GetString(CompressedAscii.Decompress(this.value));

	public static Text FromValue(string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var encodedValue = CompressedAscii.Compress(Ascii.GetBytes(value));

		if (encodedValue.Length > ushort.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Text(encodedValue);
	}

	public static Text FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var length = (ushort)buffer.ReadUnsignedBits(LongCountBitCount);
		var value = new byte[length];

		for (var index = 0; index < value.Length; index++)
		{
			value[index] = (byte)buffer.ReadUnsignedBits(Word8BitCount);
		}

		var text = new Text(value);
		_ = text.Value;

		return text;
	}

	public byte[] ToWireValue()
	{
		var wireValue = new byte[this.value.Length + LongCountByteCount];
		wireValue[MostSignificantLengthByteIndex] = (byte)(this.value.Length >> Word8BitCount);
		wireValue[LeastSignificantLengthByteIndex] = (byte)this.value.Length;
		this.value.CopyTo(wireValue, LongCountByteCount);

		return wireValue;
	}

}
