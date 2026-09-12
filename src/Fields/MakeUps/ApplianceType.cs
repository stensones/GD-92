namespace Stensones.GD92.Fields;

public sealed record ApplianceType : IGD9Field
{
	private const int WireByteCount = 3;
	private const int CharacterBitCount = 8;
	private const char PaddingCharacter = ' ';

	private ApplianceType(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static ApplianceType FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length > WireByteCount ||
			value.Value.Any(character => !char.IsAsciiLetterOrDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ApplianceType(value);
	}

	public static ApplianceType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var characters = new char[WireByteCount];

		for (var index = 0; index < characters.Length; index++)
		{
			characters[index] = (char)buffer.ReadUnsignedBits(CharacterBitCount);
		}

		return FromValue(SevenBitAsciiString.FromValue(new string(characters).TrimEnd(PaddingCharacter)));
	}

	public byte[] ToWireValue()
	{
		return SevenBitAsciiString.FromValue(this.Value.Value.PadRight(WireByteCount, PaddingCharacter)).ToWireValue();
	}
}
