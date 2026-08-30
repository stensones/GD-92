namespace Stensones.GD92.Fields;

public readonly record struct MessagePriorityLevel
{
	private MessagePriorityLevel(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static MessagePriorityLevel FromValue(byte value)
	{
		if (value is < 1 or > 9)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessagePriorityLevel(value);
	}
}
