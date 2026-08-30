namespace Stensones.GD92.Fields;

public readonly record struct MessageSequenceIdentifier
{
	private MessageSequenceIdentifier(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageSequenceIdentifier FromValue(ushort value)
	{
		if (value > 32767)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageSequenceIdentifier(value);
	}
}
