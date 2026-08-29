namespace Stensones.GD92.Fields;

public sealed record OfBlocks : Word8
{
	private OfBlocks(byte value)
		: base(value)
	{
	}

	public new static OfBlocks FromValue(byte value)
	{
		return new OfBlocks(value);
	}
}
