namespace Stensones.GD92.Fields;

public sealed record CountAndLength : IGD9Field
{
	private const int PackedFieldBitCount = 16;
	private const int ByteBitCount = 8;
	private const int DestinationCountBitCount = 6;
	private const ushort DestinationCountMask = 0x003F;

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
		var value = (ushort)buffer.ReadUnsignedBits(PackedFieldBitCount);

		return FromValues(
			MessageLength.FromValue(MessageByteLength.FromValue((ushort)(value >> DestinationCountBitCount))),
			DestinationCount.FromValue(DestinationAddressCount.FromValue((byte)(value & DestinationCountMask))));
	}

	public byte[] ToWireValue()
	{
		var value = (ushort)((this.MessageLength.Value << DestinationCountBitCount) | this.DestinationCount.Value);

		return [(byte)(value >> ByteBitCount), (byte)value];
	}
}
