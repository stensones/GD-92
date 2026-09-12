namespace Stensones.GD92.Fields;

public sealed record StopCode : IGD9Field
{
	private const int WireByteCount = 5;

	private StopCode(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static StopCode FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length != WireByteCount ||
			value.Value.Any(character => !char.IsAsciiLetterOrDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new StopCode(value);
	}

	public static StopCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var characters = new char[WireByteCount];

		for (var index = 0; index < characters.Length; index++)
		{
			characters[index] = (char)buffer.ReadUnsignedBits(8);
		}

		return FromValue(SevenBitAsciiString.FromValue(new string(characters)));
	}

	public byte[] ToWireValue()
	{
		return this.Value.ToWireValue();
	}
}
