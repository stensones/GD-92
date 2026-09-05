namespace Stensones.GD92.Fields;

public sealed record PasswordParameter : IGD9Field
{
	private PasswordParameter(
		PasswordLevel level,
		Password password,
		CommunicationsAddress communicationsAddress)
	{
		this.Level = level;
		this.Password = password;
		this.CommunicationsAddress = communicationsAddress;
	}

	public PasswordLevel Level { get; }
	public Password Password { get; }
	public CommunicationsAddress CommunicationsAddress { get; }

	public static PasswordParameter FromFields(
		PasswordLevel level,
		Password password,
		CommunicationsAddress communicationsAddress)
	{
		ArgumentNullException.ThrowIfNull(level);
		ArgumentNullException.ThrowIfNull(password);
		ArgumentNullException.ThrowIfNull(communicationsAddress);

		return new PasswordParameter(level, password, communicationsAddress);
	}

	public static PasswordParameter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			PasswordLevel.FromEncodedMessageBuffer(ref buffer),
			Password.FromEncodedMessageBuffer(ref buffer),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Level.ToWireValue(),
			.. this.Password.ToWireValue(),
			.. this.CommunicationsAddress.ToWireValue()
		];
	}
}
