using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record Text : IGD92Message
{
	private Text(Block block, OfBlocks ofBlocks, FieldText text)
	{
		this.Block = block;
		this.OfBlocks = ofBlocks;
		this.MessageText = text;
	}

	public Block Block { get; }
	public OfBlocks OfBlocks { get; }
	public FieldText MessageText { get; }

	public static Text FromFields(Block block, OfBlocks ofBlocks, FieldText text)
	{
		ArgumentNullException.ThrowIfNull(block);
		ArgumentNullException.ThrowIfNull(ofBlocks);
		ArgumentNullException.ThrowIfNull(text);

		return new Text(block, ofBlocks, text);
	}

	public byte[] ToWireValue()
	{
		return [.. this.Block.ToWireValue(), .. this.OfBlocks.ToWireValue(), .. this.MessageText.ToWireValue()];
	}
}
