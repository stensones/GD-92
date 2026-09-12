namespace Stensones.GD92.Fields;

public sealed record AddressRange : IGD9Field
{
	private AddressRange(CommunicationsAddress firstAddress, CommunicationsAddress lastAddress)
	{
		this.FirstAddress = firstAddress;
		this.LastAddress = lastAddress;
	}

	public CommunicationsAddress FirstAddress { get; }
	public CommunicationsAddress LastAddress { get; }

	public static AddressRange FromValues(
		CommunicationsAddress firstAddress,
		CommunicationsAddress lastAddress)
	{
		ArgumentNullException.ThrowIfNull(firstAddress);
		ArgumentNullException.ThrowIfNull(lastAddress);

		return new AddressRange(firstAddress, lastAddress);
	}

	public static AddressRange FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [.. this.FirstAddress.ToWireValue(), .. this.LastAddress.ToWireValue()];
	}
}
