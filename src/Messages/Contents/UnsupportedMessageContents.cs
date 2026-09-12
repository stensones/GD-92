using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class UnsupportedMessageContents : IGD92MessageContents
{
	private readonly byte[] wireValue;

	private UnsupportedMessageContents(MessageType type, byte[] wireValue)
	{
		this.Type = type;
		this.wireValue = wireValue;
	}

	public MessageType Type { get; }

	internal static UnsupportedMessageContents FromWireValue(
		MessageType type,
		ReadOnlySpan<byte> wireValue)
	{
		ArgumentNullException.ThrowIfNull(type);

		return new UnsupportedMessageContents(type, wireValue.ToArray());
	}

	public byte[] ToWireValue()
	{
		return this.wireValue.ToArray();
	}
}
