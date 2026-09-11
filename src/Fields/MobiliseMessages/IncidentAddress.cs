namespace Stensones.GD92.Fields;

public sealed record IncidentAddress : IGD9Field
{
	private const int MaximumAddressTextLength = 120;
	private const int MaximumHouseNumberLength = 10;
	private const int MaximumStreetLength = 40;
	private const int MaximumSubDistrictLength = 30;
	private const int MaximumDistrictLength = 30;
	private const int MaximumTownLength = 30;
	private const int MaximumCountyLength = 20;
	private const int MaximumPostcodeLength = 10;

	private IncidentAddress(
		SevenBitAsciiString addressText,
		SevenBitAsciiString houseNumber,
		SevenBitAsciiString street,
		SevenBitAsciiString subDistrict,
		SevenBitAsciiString district,
		SevenBitAsciiString town,
		SevenBitAsciiString county,
		SevenBitAsciiString postcode)
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

	public SevenBitAsciiString AddressText { get; }
	public SevenBitAsciiString HouseNumber { get; }
	public SevenBitAsciiString Street { get; }
	public SevenBitAsciiString SubDistrict { get; }
	public SevenBitAsciiString District { get; }
	public SevenBitAsciiString Town { get; }
	public SevenBitAsciiString County { get; }
	public SevenBitAsciiString Postcode { get; }

	public static IncidentAddress FromValues(
		SevenBitAsciiString addressText,
		SevenBitAsciiString houseNumber,
		SevenBitAsciiString street,
		SevenBitAsciiString subDistrict,
		SevenBitAsciiString district,
		SevenBitAsciiString town,
		SevenBitAsciiString county,
		SevenBitAsciiString postcode)
	{
		var address = new IncidentAddress(
			addressText,
			houseNumber,
			street,
			subDistrict,
			district,
			town,
			county,
			postcode);

		_ = address.ToWireValue();

		return address;
	}

	public static IncidentAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumAddressTextLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumHouseNumberLength, false),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumStreetLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumSubDistrictLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumDistrictLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumTownLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumCountyLength, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, MaximumPostcodeLength, false));
	}

	public byte[] ToWireValue()
	{
		return [
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.AddressText, MaximumAddressTextLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.HouseNumber, MaximumHouseNumberLength, false),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Street, MaximumStreetLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.SubDistrict, MaximumSubDistrictLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.District, MaximumDistrictLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Town, MaximumTownLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.County, MaximumCountyLength, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Postcode, MaximumPostcodeLength, false)
		];
	}
}
