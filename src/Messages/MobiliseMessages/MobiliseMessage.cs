using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record MobiliseMessage : IGD92MessageContents
{
	private static readonly MessageType MobiliseMessageType =
		MessageType.FromValue(GD92MessageType.MobiliseMessage);

	private MobiliseMessage(
		Block block,
		OfBlocks ofBlocks,
		ManualAcknowledgementRequest manualAcknowledgementRequest,
		TimeAndDate timeAndDate,
		CallsignList callsignList,
		IReadOnlyList<IncidentDetails> incidentDetails)
	{
		this.Block = block;
		this.OfBlocks = ofBlocks;
		this.ManualAcknowledgementRequest = manualAcknowledgementRequest;
		this.TimeAndDate = timeAndDate;
		this.CallsignList = callsignList;
		this.IncidentDetails = incidentDetails;
	}

	public Block Block { get; }
	public OfBlocks OfBlocks { get; }
	public ManualAcknowledgementRequest ManualAcknowledgementRequest { get; }
	public TimeAndDate TimeAndDate { get; }
	public CallsignList CallsignList { get; }
	public IReadOnlyList<IncidentDetails> IncidentDetails { get; }
	public MessageType Type => MobiliseMessageType;

	public static MobiliseMessage FromFields(
		Block block,
		OfBlocks ofBlocks,
		ManualAcknowledgementRequest manualAcknowledgementRequest,
		TimeAndDate timeAndDate,
		CallsignList callsignList,
		params IncidentDetails[] incidentDetails)
	{
		ArgumentNullException.ThrowIfNull(block);
		ArgumentNullException.ThrowIfNull(ofBlocks);
		ArgumentNullException.ThrowIfNull(manualAcknowledgementRequest);
		ArgumentNullException.ThrowIfNull(timeAndDate);
		ArgumentNullException.ThrowIfNull(callsignList);
		ArgumentNullException.ThrowIfNull(incidentDetails);

		if (incidentDetails.Any(details => details is null))
		{
			throw new ArgumentException("Incident details may not contain null values.", nameof(incidentDetails));
		}

		return new MobiliseMessage(
			block,
			ofBlocks,
			manualAcknowledgementRequest,
			timeAndDate,
			callsignList,
			Array.AsReadOnly(incidentDetails.ToArray()));
	}

	public static MobiliseMessage FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var block = Block.FromEncodedMessageBuffer(ref buffer);
		var ofBlocks = OfBlocks.FromEncodedMessageBuffer(ref buffer);
		var manualAcknowledgementRequest = ManualAcknowledgementRequest.FromEncodedMessageBuffer(ref buffer);
		var timeAndDate = TimeAndDate.FromEncodedMessageBuffer(ref buffer);
		var callsignList = CallsignList.FromEncodedMessageBuffer(ref buffer);
		var incidentDetails = new List<IncidentDetails>();

		while (buffer.RemainingBitCount > 0)
		{
			incidentDetails.Add(global::Stensones.GD92.Messages.IncidentDetails.FromEncodedMessageBuffer(ref buffer));
		}

		return FromFields(
			block,
			ofBlocks,
			manualAcknowledgementRequest,
			timeAndDate,
			callsignList,
			[.. incidentDetails]);
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte>();

		wireValue.AddRange(this.Block.ToWireValue());
		wireValue.AddRange(this.OfBlocks.ToWireValue());
		wireValue.AddRange(this.ManualAcknowledgementRequest.ToWireValue());
		wireValue.AddRange(this.TimeAndDate.ToWireValue());
		wireValue.AddRange(this.CallsignList.ToWireValue());

		foreach (var incidentDetails in this.IncidentDetails)
		{
			wireValue.AddRange(incidentDetails.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
