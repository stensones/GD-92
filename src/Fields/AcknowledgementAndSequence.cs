namespace Stensones.GD92.Fields;

public sealed record AcknowledgementAndSequence
{
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

	public byte[] ToWireValue()
	{
		var value = this.SequenceNumber.Value;

		if (this.AcknowledgementRequest.IsRequested)
		{
			value |= 0x8000;
		}

		return [(byte)(value >> 8), (byte)value];
	}
}
