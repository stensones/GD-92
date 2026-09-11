namespace Stensones.GD92.Fields;

public readonly record struct MessageByteLength
{
	private const ushort MaximumValue = 1023;

	private MessageByteLength(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageByteLength FromValue(ushort value)
	{
		if (value > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageByteLength(value);
	}
}
