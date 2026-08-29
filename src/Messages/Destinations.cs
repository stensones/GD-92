using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class Destinations
{
	private readonly IReadOnlyList<CommunicationsAddress> addresses;

	private Destinations(IReadOnlyList<CommunicationsAddress> addresses)
	{
		this.addresses = addresses;
		this.Count = DestinationCount.FromValue((byte)addresses.Count);
	}

	public IReadOnlyList<CommunicationsAddress> Addresses => this.addresses;
	public DestinationCount Count { get; }

	public static Destinations FromAddresses(params CommunicationsAddress[] addresses)
	{
		ArgumentNullException.ThrowIfNull(addresses);

		if (addresses.Length is < 1 or > 63)
		{
			throw new ArgumentOutOfRangeException(nameof(addresses));
		}

		foreach (var address in addresses)
		{
			ArgumentNullException.ThrowIfNull(address);
		}

		if (addresses.Distinct().Count() != addresses.Length)
		{
			throw new ArgumentException("Destination addresses must be unique.", nameof(addresses));
		}

		return new Destinations(Array.AsReadOnly(addresses.ToArray()));
	}

	public byte[] ToWireValue()
	{
		return [.. this.addresses.SelectMany(address => address.ToWireValue())];
	}
}
