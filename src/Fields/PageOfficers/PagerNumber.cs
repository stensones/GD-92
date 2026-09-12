namespace Stensones.GD92.Fields;

public sealed record PagerNumber : IGD9Field
{
	private PagerNumber(TelephoneNumber telephoneNumber, PagerType pagerType)
	{
		this.TelephoneNumber = telephoneNumber;
		this.PagerType = pagerType;
	}

	public TelephoneNumber TelephoneNumber { get; }
	public PagerType PagerType { get; }

	public static PagerNumber FromValues(TelephoneNumber telephoneNumber, PagerType pagerType)
	{
		ArgumentNullException.ThrowIfNull(telephoneNumber);
		ArgumentNullException.ThrowIfNull(pagerType);
		return new PagerNumber(telephoneNumber, pagerType);
	}

	public static PagerNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValues(
			TelephoneNumber.FromEncodedMessageBuffer(ref buffer),
			PagerType.FromEncodedMessageBuffer(ref buffer));

	public byte[] ToWireValue() => [.. this.TelephoneNumber.ToWireValue(), .. this.PagerType.ToWireValue()];
}
