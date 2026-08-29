namespace Stensones.GD92.Fields;

public sealed record Node
{
	private Node(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static Node FromValue(ushort value)
	{
		if (value > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Node(value);
	}
}
