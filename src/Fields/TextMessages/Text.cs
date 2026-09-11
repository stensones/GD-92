using System.Text;

namespace Stensones.GD92.Fields;

public sealed record Text : IGD9Field
{
	private static readonly Encoding Ascii = Encoding.GetEncoding(
		"us-ascii",
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
		var length = (ushort)buffer.ReadUnsignedBits(16);
		var value = new byte[length];

		for (var index = 0; index < value.Length; index++)
		{
			value[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		var text = new Text(value);
		_ = text.Value;

		return text;
	}

	public byte[] ToWireValue()
	{
		var wireValue = new byte[this.value.Length + 2];
		wireValue[0] = (byte)(this.value.Length >> 8);
		wireValue[1] = (byte)this.value.Length;
		this.value.CopyTo(wireValue, 2);

		return wireValue;
	}

}
