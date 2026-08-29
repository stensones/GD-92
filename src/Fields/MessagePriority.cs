namespace Stensones.GD92.Fields;

public sealed record MessagePriority
{
	private MessagePriority(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static MessagePriority FromValue(byte value)
	{
		if (value is < 1 or > 9)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessagePriority(value);
	}
}
