using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record DutyStaffingEntry
{
	private DutyStaffingEntry(
		Callsign callsign,
		OfficerInCharge officerInCharge,
		Riders riders,
		StatusCode statusCode,
		Remarks remarks)
	{
		this.Callsign = callsign;
		this.OfficerInCharge = officerInCharge;
		this.Riders = riders;
		this.StatusCode = statusCode;
		this.Remarks = remarks;
	}

	public Callsign Callsign { get; }
	public OfficerInCharge OfficerInCharge { get; }
	public Riders Riders { get; }
	public StatusCode StatusCode { get; }
	public Remarks Remarks { get; }

	public static DutyStaffingEntry FromFields(
		Callsign callsign,
		OfficerInCharge officerInCharge,
		Riders riders,
		StatusCode statusCode,
		Remarks remarks)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(officerInCharge);
		ArgumentNullException.ThrowIfNull(riders);
		ArgumentNullException.ThrowIfNull(statusCode);
		ArgumentNullException.ThrowIfNull(remarks);

		return new DutyStaffingEntry(callsign, officerInCharge, riders, statusCode, remarks);
	}

	public static DutyStaffingEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			Callsign.FromEncodedMessageBuffer(ref buffer),
			OfficerInCharge.FromEncodedMessageBuffer(ref buffer),
			Riders.FromEncodedMessageBuffer(ref buffer),
			StatusCode.FromEncodedMessageBuffer(ref buffer),
			Remarks.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Callsign.ToWireValue(),
			.. this.OfficerInCharge.ToWireValue(),
			.. this.Riders.ToWireValue(),
			.. this.StatusCode.ToWireValue(),
			.. this.Remarks.ToWireValue()
		];
	}
}
