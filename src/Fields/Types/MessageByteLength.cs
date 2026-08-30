namespace Stensones.GD92.Fields;

public readonly record struct MessageByteLength
{
	private MessageByteLength(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageByteLength FromValue(ushort value)
	{
		if (value > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageByteLength(value);
	}
}
