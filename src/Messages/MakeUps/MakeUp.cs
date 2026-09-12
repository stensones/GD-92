using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record MakeUp : IGD92MessageContents
{
	private static readonly MessageType MakeUpMessageType =
		MessageType.FromValue(GD92MessageType.MakeUp);

	private MakeUp(
		Callsign callsign,
		IncidentNumber incidentNumber,
		IReadOnlyList<MakeUpAppliance> appliances)
	{
		this.Callsign = callsign;
		this.IncidentNumber = incidentNumber;
		this.Appliances = appliances;
	}

	public Callsign Callsign { get; }
	public IncidentNumber IncidentNumber { get; }
	public IReadOnlyList<MakeUpAppliance> Appliances { get; }
	public MessageType Type => MakeUpMessageType;

	public static MakeUp FromFields(
		Callsign callsign,
		IncidentNumber incidentNumber,
		params MakeUpAppliance[] appliances)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(incidentNumber);
		ArgumentNullException.ThrowIfNull(appliances);

		if (appliances.Any(appliance => appliance is null))
		{
			throw new ArgumentException("Make-up appliances may not contain null values.", nameof(appliances));
		}

		NumberTypes.FromValue(checked((byte)appliances.Length));

		return new MakeUp(callsign, incidentNumber, Array.AsReadOnly(appliances.ToArray()));
	}

	public static MakeUp FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var callsign = Callsign.FromEncodedMessageBuffer(ref buffer);
		var incidentNumber = IncidentNumber.FromEncodedMessageBuffer(ref buffer);
		var numberTypes = NumberTypes.FromEncodedMessageBuffer(ref buffer);
		var appliances = new MakeUpAppliance[numberTypes.Value];

		for (var index = 0; index < appliances.Length; index++)
		{
			appliances[index] = MakeUpAppliance.FromEncodedMessageBuffer(ref buffer);
		}

		return FromFields(callsign, incidentNumber, appliances);
	}

	public byte[] ToWireValue()
	{
		var numberTypes = NumberTypes.FromValue(checked((byte)this.Appliances.Count));
		var wireValue = new List<byte>();

		wireValue.AddRange(this.Callsign.ToWireValue());
		wireValue.AddRange(this.IncidentNumber.ToWireValue());
		wireValue.AddRange(numberTypes.ToWireValue());

		foreach (var appliance in this.Appliances)
		{
			wireValue.AddRange(appliance.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
