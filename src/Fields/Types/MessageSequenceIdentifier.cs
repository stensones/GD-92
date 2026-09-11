namespace Stensones.GD92.Fields;

public readonly record struct MessageSequenceIdentifier
{
	private const ushort MaximumValue = 32767;

	private MessageSequenceIdentifier(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageSequenceIdentifier FromValue(ushort value)
	{
		if (value > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageSequenceIdentifier(value);
	}
}
