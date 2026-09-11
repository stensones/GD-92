namespace Stensones.GD92.Fields;

public sealed record IncidentAddress : IGD9Field
{
	private IncidentAddress(
		AddressText addressText,
		HouseNumber houseNumber,
		Street street,
		SubDistrict subDistrict,
		District district,
		Town town,
		County county,
		Postcode postcode)
	{
		this.AddressText = addressText;
		this.HouseNumber = houseNumber;
		this.Street = street;
		this.SubDistrict = subDistrict;
		this.District = district;
		this.Town = town;
		this.County = county;
		this.Postcode = postcode;
	}

	public AddressText AddressText { get; }
	public HouseNumber HouseNumber { get; }
	public Street Street { get; }
	public SubDistrict SubDistrict { get; }
	public District District { get; }
	public Town Town { get; }
	public County County { get; }
	public Postcode Postcode { get; }

	public static IncidentAddress FromValues(
		AddressText addressText,
		HouseNumber houseNumber,
		Street street,
		SubDistrict subDistrict,
		District district,
		Town town,
		County county,
		Postcode postcode)
	{
		return new IncidentAddress(
			addressText,
			houseNumber,
			street,
			subDistrict,
			district,
			town,
			county,
			postcode);
	}

	public static IncidentAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			AddressText.FromEncodedMessageBuffer(ref buffer),
			HouseNumber.FromEncodedMessageBuffer(ref buffer),
			Street.FromEncodedMessageBuffer(ref buffer),
			SubDistrict.FromEncodedMessageBuffer(ref buffer),
			District.FromEncodedMessageBuffer(ref buffer),
			Town.FromEncodedMessageBuffer(ref buffer),
			County.FromEncodedMessageBuffer(ref buffer),
			Postcode.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.AddressText.ToWireValue(),
			.. this.HouseNumber.ToWireValue(),
			.. this.Street.ToWireValue(),
			.. this.SubDistrict.ToWireValue(),
			.. this.District.ToWireValue(),
			.. this.Town.ToWireValue(),
			.. this.County.ToWireValue(),
			.. this.Postcode.ToWireValue()
		];
	}
}
