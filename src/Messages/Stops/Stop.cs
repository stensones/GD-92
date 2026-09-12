using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record Stop : IGD92MessageContents
{
	private static readonly MessageType StopMessageType =
		MessageType.FromValue(GD92MessageType.Stop);

	private Stop(Callsign callsign, IncidentNumber incidentNumber, StopCode stopCode)
	{
		this.Callsign = callsign;
		this.IncidentNumber = incidentNumber;
		this.StopCode = stopCode;
	}

	public Callsign Callsign { get; }
	public IncidentNumber IncidentNumber { get; }
	public StopCode StopCode { get; }
	public MessageType Type => StopMessageType;

	public static Stop FromFields(Callsign callsign, IncidentNumber incidentNumber, StopCode stopCode)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(incidentNumber);
		ArgumentNullException.ThrowIfNull(stopCode);

		return new Stop(callsign, incidentNumber, stopCode);
	}

	public static Stop FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			Callsign.FromEncodedMessageBuffer(ref buffer),
			IncidentNumber.FromEncodedMessageBuffer(ref buffer),
			StopCode.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Callsign.ToWireValue(),
			.. this.IncidentNumber.ToWireValue(),
			.. this.StopCode.ToWireValue()
		];
	}
}
