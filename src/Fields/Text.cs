using System.Text;

namespace Stensones.GD92.Fields;

public sealed record Text
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

	public static Text FromValue(string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var encodedValue = Ascii.GetBytes(value);

		if (encodedValue.Length > ushort.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Text(encodedValue);
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
