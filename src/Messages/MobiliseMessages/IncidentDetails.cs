using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record IncidentDetails
{
	private IncidentDetails(
		IncidentNumber incidentNumber,
		MobilisationType mobilisationType,
		IncidentAddress address,
		MapReference mapReference,
		TelephoneNumber telephoneNumber,
		FieldText text)
	{
		this.IncidentNumber = incidentNumber;
		this.MobilisationType = mobilisationType;
		this.Address = address;
		this.MapReference = mapReference;
		this.TelephoneNumber = telephoneNumber;
		this.Text = text;
	}

	public IncidentNumber IncidentNumber { get; }
	public MobilisationType MobilisationType { get; }
	public IncidentAddress Address { get; }
	public MapReference MapReference { get; }
	public TelephoneNumber TelephoneNumber { get; }
	public FieldText Text { get; }

	public static IncidentDetails FromFields(
		IncidentNumber incidentNumber,
		MobilisationType mobilisationType,
		IncidentAddress address,
		MapReference mapReference,
		TelephoneNumber telephoneNumber,
		FieldText text)
	{
		ArgumentNullException.ThrowIfNull(incidentNumber);
		ArgumentNullException.ThrowIfNull(mobilisationType);
		ArgumentNullException.ThrowIfNull(address);
		ArgumentNullException.ThrowIfNull(mapReference);
		ArgumentNullException.ThrowIfNull(telephoneNumber);
		ArgumentNullException.ThrowIfNull(text);

		return new IncidentDetails(
			incidentNumber,
			mobilisationType,
			address,
			mapReference,
			telephoneNumber,
			text);
	}

	public static IncidentDetails FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			IncidentNumber.FromEncodedMessageBuffer(ref buffer),
			MobilisationType.FromEncodedMessageBuffer(ref buffer),
			IncidentAddress.FromEncodedMessageBuffer(ref buffer),
			MapReference.FromEncodedMessageBuffer(ref buffer),
			TelephoneNumber.FromEncodedMessageBuffer(ref buffer),
			FieldText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.IncidentNumber.ToWireValue(),
			.. this.MobilisationType.ToWireValue(),
			.. this.Address.ToWireValue(),
			.. this.MapReference.ToWireValue(),
			.. this.TelephoneNumber.ToWireValue(),
			.. this.Text.ToWireValue()
		];
	}
}
