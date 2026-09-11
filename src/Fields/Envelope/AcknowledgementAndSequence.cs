namespace Stensones.GD92.Fields;

public sealed record AcknowledgementAndSequence : IGD9Field
{
	private const int PackedFieldBitCount = 16;
	private const int ByteBitCount = 8;
	private const ushort SequenceNumberMask = 0x7FFF;
	private const ushort AcknowledgementRequestMask = 0x8000;

	private AcknowledgementAndSequence(SequenceNumber sequenceNumber, AcknowledgementRequest acknowledgementRequest)
	{
		this.SequenceNumber = sequenceNumber;
		this.AcknowledgementRequest = acknowledgementRequest;
	}

	public SequenceNumber SequenceNumber { get; }
	public AcknowledgementRequest AcknowledgementRequest { get; }

	public static AcknowledgementAndSequence FromValues(
		SequenceNumber sequenceNumber,
		AcknowledgementRequest acknowledgementRequest)
	{
		ArgumentNullException.ThrowIfNull(sequenceNumber);
		ArgumentNullException.ThrowIfNull(acknowledgementRequest);

		return new AcknowledgementAndSequence(sequenceNumber, acknowledgementRequest);
	}

	public static AcknowledgementAndSequence FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var value = (ushort)buffer.ReadUnsignedBits(PackedFieldBitCount);

		return FromValues(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue((ushort)(value & SequenceNumberMask))),
			(value & AcknowledgementRequestMask) != 0
				? AcknowledgementRequest.Requested
				: AcknowledgementRequest.NotRequested);
	}

	public byte[] ToWireValue()
	{
		var value = this.SequenceNumber.Value;

		if (this.AcknowledgementRequest.IsRequested)
		{
			value |= AcknowledgementRequestMask;
		}

		return [(byte)(value >> ByteBitCount), (byte)value];
	}
}
