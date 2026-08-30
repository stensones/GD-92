namespace Stensones.GD92.Fields;

public sealed record Node
{
	private Node(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static Node FromValue(NodeIdentifier value)
	{
		return new Node(value.Value);
	}
}
