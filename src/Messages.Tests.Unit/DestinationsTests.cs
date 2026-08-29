using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class DestinationsTests
{
	[Fact]
	public void Serializes_addresses_in_their_declared_order()
	{
		var destinations = Destinations.FromAddresses(
			CreateAddress(26, 100, 25),
			CreateAddress(26, 101, 26));

		destinations.ToWireValue().Should().Equal(Convert.FromHexString("1A19191A195A"));
		destinations.Count.Value.Should().Be((byte)2);
	}

	[Fact]
	public void Rejects_duplicate_addresses()
	{
		var address = CreateAddress(26, 100, 25);
		var createDestinations = () => Destinations.FromAddresses(address, address);

		createDestinations.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(64)]
	public void Requires_between_one_and_63_destinations(int count)
	{
		var address = CreateAddress(26, 100, 25);
		var addresses = Enumerable.Repeat(address, count).ToArray();
		var createDestinations = () => Destinations.FromAddresses(addresses);

		createDestinations.Should().Throw<ArgumentOutOfRangeException>();
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(brigade),
			Node.FromValue(node),
			Port.FromValue(port));
	}
}
