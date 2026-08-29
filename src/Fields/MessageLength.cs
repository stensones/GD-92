namespace Stensones.GD92.Fields;

public sealed record MessageLength
{
	private MessageLength(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static MessageLength FromValue(ushort value)
	{
		if (value > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageLength(value);
	}
}
