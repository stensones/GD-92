using System.Text;

namespace Stensones.GD92.Fields;

public readonly record struct SevenBitAsciiString
{
	private const char MaximumAsciiCharacter = (char)0x7F;

	private static readonly Encoding Ascii = Encoding.ASCII;

	private SevenBitAsciiString(string value)
	{
		this.Value = value;
	}

	public string Value { get; }

	public static SevenBitAsciiString FromValue(string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (value.Any(character => character > MaximumAsciiCharacter))
		{
			throw new ArgumentException("Value may contain only 7-bit ASCII characters.", nameof(value));
		}

		return new SevenBitAsciiString(value);
	}

	public byte[] ToWireValue()
	{
		return Ascii.GetBytes(this.Value);
	}
}
