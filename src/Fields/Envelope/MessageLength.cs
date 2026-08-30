namespace Stensones.GD92.Fields;

public sealed record MessageLength
{
	private MessageLength(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageLength FromValue(MessageByteLength value)
	{
		return new MessageLength(value.Value);
	}
}
