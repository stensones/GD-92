namespace Stensones.GD92.Fields;

public sealed record AlternativeAddressTableEntry : IGD9Field
{
	private AlternativeAddressTableEntry(AddressRange addressRange, AddressString addressString)
	{
		this.AddressRange = addressRange;
		this.AddressString = addressString;
	}

	public AddressRange AddressRange { get; }
	public AddressString AddressString { get; }

	public static AlternativeAddressTableEntry FromValues(
		AddressRange addressRange,
		AddressString addressString)
	{
		ArgumentNullException.ThrowIfNull(addressRange);
		ArgumentNullException.ThrowIfNull(addressString);

		return new AlternativeAddressTableEntry(addressRange, addressString);
	}

	public static AlternativeAddressTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			AddressRange.FromEncodedMessageBuffer(ref buffer),
			AddressString.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [.. this.AddressRange.ToWireValue(), .. this.AddressString.ToWireValue()];
	}
}
