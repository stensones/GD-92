namespace Stensones.GD92.Fields;

public sealed record SequenceNumber
{
	private SequenceNumber(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static SequenceNumber FromValue(MessageSequenceIdentifier value)
	{
		return new SequenceNumber(value.Value);
	}
}
