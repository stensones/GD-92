namespace Stensones.GD92.Fields;

public sealed record IncidentAddress : IGD9Field
{
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
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 120, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 10, false),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 40, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 30, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 30, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 30, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 20, true),
			MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 10, false));
	}

	public byte[] ToWireValue()
	{
		return [
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.AddressText, 120, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.HouseNumber, 10, false),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Street, 40, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.SubDistrict, 30, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.District, 30, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Town, 30, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.County, 20, true),
			.. MobiliseMessageStringEncoding.ToCountedWireValue(this.Postcode, 10, false)
		];
	}
}
