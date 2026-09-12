using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record LogUpdate : IGD92MessageContents
{
	private static readonly MessageType LogUpdateMessageType =
		MessageType.FromValue(GD92MessageType.LogUpdate);

	private LogUpdate(Callsign callsign, IncidentNumber incidentNumber, Update update)
	{
		this.Callsign = callsign;
		this.IncidentNumber = incidentNumber;
		this.Update = update;
	}

	public Callsign Callsign { get; }
	public IncidentNumber IncidentNumber { get; }
	public Update Update { get; }
	public MessageType Type => LogUpdateMessageType;

	public static LogUpdate FromFields(Callsign callsign, IncidentNumber incidentNumber, Update update)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(incidentNumber);
		ArgumentNullException.ThrowIfNull(update);

		return new LogUpdate(callsign, incidentNumber, update);
	}

	public static LogUpdate FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			Callsign.FromEncodedMessageBuffer(ref buffer),
			IncidentNumber.FromEncodedMessageBuffer(ref buffer),
			Update.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Callsign.ToWireValue(),
			.. this.IncidentNumber.ToWireValue(),
			.. this.Update.ToWireValue()
		];
	}
}
