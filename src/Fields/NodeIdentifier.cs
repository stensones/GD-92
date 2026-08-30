namespace Stensones.GD92.Fields;

public readonly record struct NodeIdentifier
{
	private NodeIdentifier(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static NodeIdentifier FromValue(ushort value)
	{
		if (value > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new NodeIdentifier(value);
	}
}
