namespace Stensones.GD92.Fields;

public sealed class Password : IGD9Field
{
	private const int StringLengthBitCount = 8;

	private Password(PasswordValue value)
	{
		this.Value = value;
	}

	public PasswordValue Value { get; }

	public static Password FromValue(PasswordValue value)
	{
		return new Password(value);
	}

	public static Password FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var length = (byte)buffer.ReadUnsignedBits(StringLengthBitCount);

		var characters = new byte[length];

		for (var index = 0; index < characters.Length; index++)
		{
			characters[index] = (byte)buffer.ReadUnsignedBits(StringLengthBitCount);
		}

		var charactersAsString = new string(characters.Select(character => (char)character).ToArray());

		return FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue(charactersAsString)));
	}

	public byte[] ToWireValue()
	{
		return [
			(byte)this.Value.Value.Value.Length,
			.. this.Value.Value.ToWireValue()
		];
	}
}
