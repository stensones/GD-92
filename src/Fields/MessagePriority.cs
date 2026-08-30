namespace Stensones.GD92.Fields;

public sealed record MessagePriority
{
	private MessagePriority(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static MessagePriority FromValue(MessagePriorityLevel value)
	{
		return new MessagePriority(value.Value);
	}
}
