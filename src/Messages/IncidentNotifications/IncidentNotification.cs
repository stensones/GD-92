using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record IncidentNotification : IGD92MessageContents
{
	private static readonly MessageType IncidentNotificationMessageType =
		MessageType.FromValue(GD92MessageType.IncidentNotification);

	private IncidentNotification(
		AlarmType alarmType,
		CallAgency callAgency,
		TelephoneNumber telephoneNumber,
		AlarmReference alarmReference,
		AlarmSerial alarmSerial,
		IncidentAddress address,
		FieldText text)
	{
		this.AlarmType = alarmType;
		this.CallAgency = callAgency;
		this.TelephoneNumber = telephoneNumber;
		this.AlarmReference = alarmReference;
		this.AlarmSerial = alarmSerial;
		this.Address = address;
		this.Text = text;
	}

	public AlarmType AlarmType { get; }
	public CallAgency CallAgency { get; }
	public TelephoneNumber TelephoneNumber { get; }
	public AlarmReference AlarmReference { get; }
	public AlarmSerial AlarmSerial { get; }
	public IncidentAddress Address { get; }
	public FieldText Text { get; }
	public MessageType Type => IncidentNotificationMessageType;

	public static IncidentNotification FromFields(
		AlarmType alarmType,
		CallAgency callAgency,
		TelephoneNumber telephoneNumber,
		AlarmReference alarmReference,
		AlarmSerial alarmSerial,
		IncidentAddress address,
		FieldText text)
	{
		ArgumentNullException.ThrowIfNull(alarmType);
		ArgumentNullException.ThrowIfNull(callAgency);
		ArgumentNullException.ThrowIfNull(telephoneNumber);
		ArgumentNullException.ThrowIfNull(alarmReference);
		ArgumentNullException.ThrowIfNull(alarmSerial);
		ArgumentNullException.ThrowIfNull(address);
		ArgumentNullException.ThrowIfNull(text);

		return new IncidentNotification(
			alarmType,
			callAgency,
			telephoneNumber,
			alarmReference,
			alarmSerial,
			address,
			text);
	}

	public static IncidentNotification FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			AlarmType.FromEncodedMessageBuffer(ref buffer),
			CallAgency.FromEncodedMessageBuffer(ref buffer),
			TelephoneNumber.FromEncodedMessageBuffer(ref buffer),
			AlarmReference.FromEncodedMessageBuffer(ref buffer),
			AlarmSerial.FromEncodedMessageBuffer(ref buffer),
			IncidentAddress.FromEncodedMessageBuffer(ref buffer),
			FieldText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.AlarmType.ToWireValue(),
			.. this.CallAgency.ToWireValue(),
			.. this.TelephoneNumber.ToWireValue(),
			.. this.AlarmReference.ToWireValue(),
			.. this.AlarmSerial.ToWireValue(),
			.. this.Address.ToWireValue(),
			.. this.Text.ToWireValue()
		];
	}
}
