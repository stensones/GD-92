namespace Stensones.GD92.Fields;

public readonly record struct MessagePriorityLevel
{
	private const byte MinimumValue = 1;
	private const byte MaximumValue = 9;

	private MessagePriorityLevel(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static MessagePriorityLevel FromValue(byte value)
	{
		if (value is < MinimumValue or > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessagePriorityLevel(value);
	}
}
