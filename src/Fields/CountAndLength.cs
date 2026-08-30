namespace Stensones.GD92.Fields;

public sealed record CountAndLength : IGD9Field
{
	private CountAndLength(MessageLength messageLength, DestinationCount destinationCount)
	{
		this.MessageLength = messageLength;
		this.DestinationCount = destinationCount;
	}

	public MessageLength MessageLength { get; }
	public DestinationCount DestinationCount { get; }

	public static CountAndLength FromValues(MessageLength messageLength, DestinationCount destinationCount)
	{
		ArgumentNullException.ThrowIfNull(messageLength);
		ArgumentNullException.ThrowIfNull(destinationCount);

		return new CountAndLength(messageLength, destinationCount);
	}

	public static CountAndLength FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var value = (ushort)buffer.ReadUnsignedBits(16);

		return FromValues(
			MessageLength.FromValue(MessageByteLength.FromValue((ushort)(value >> 6))),
			DestinationCount.FromValue(DestinationAddressCount.FromValue((byte)(value & 0x3F))));
	}

	public byte[] ToWireValue()
	{
		var value = (ushort)((this.MessageLength.Value << 6) | this.DestinationCount.Value);

		return [(byte)(value >> 8), (byte)value];
	}
}
