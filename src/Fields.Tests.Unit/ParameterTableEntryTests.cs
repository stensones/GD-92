using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterTableEntryTests
{
	[Fact]
	public void Round_trips_an_alternative_address_table_entry()
	{
		var entry = AlternativeAddressTableEntry.FromValues(
			AddressRange.FromValues(
				CommunicationsAddress.FromValues(
					Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
					Node.FromValue(NodeIdentifier.FromValue(2)),
					Port.FromValue(PortIdentifier.FromValue(3))),
				CommunicationsAddress.FromValues(
					Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
					Node.FromValue(NodeIdentifier.FromValue(4)),
					Port.FromValue(PortIdentifier.FromValue(5)))),
			AddressString.FromValue(SevenBitAsciiString.FromValue("AAAAA")));
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = AlternativeAddressTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("010083010105031B4105"));
		decodedEntry.AddressRange.FirstAddress.Node.Value.Should().Be(2);
		decodedEntry.AddressRange.LastAddress.Node.Value.Should().Be(4);
		decodedEntry.AddressString.Value.Value.Should().Be("AAAAA");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_lan_table_entry()
	{
		var encoded = LanTableEntry.FromValues(
			ParameterEntryIndex.FromValue(0x1234),
			ProtocolBoolean.True,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
				Node.FromValue(NodeIdentifier.FromValue(4)),
				Port.FromValue(PortIdentifier.FromValue(5))),
			LanAddress.FromValue(SevenBitAsciiString.FromValue("LAN")))
			.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var entry = LanTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("123401010105034C414E"));
		entry.Index.Value.Should().Be(0x1234);
		entry.Used.Should().Be(ProtocolBoolean.True);
		entry.NextNode.Node.Value.Should().Be(4);
		entry.LanAddress.Value.Value.Should().Be("LAN");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_telephone_table_entry()
	{
		var entry = TelephoneTableEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			ProtocolBoolean.False,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
				Node.FromValue(NodeIdentifier.FromValue(4)),
				Port.FromValue(PortIdentifier.FromValue(5))),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("12")),
			HoldTime.FromValue(30),
			ProtocolBoolean.True);
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = TelephoneTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("0002000101050231321E01"));
		decodedEntry.Used.Should().Be(ProtocolBoolean.False);
		decodedEntry.TelephoneNumber.Value.Value.Should().Be("12");
		decodedEntry.HoldTime.Value.Should().Be(30);
		decodedEntry.Available.Should().Be(ProtocolBoolean.True);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_mobile_data_terminal_table_entry()
	{
		var entry = MobileDataTerminalTableEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			ProtocolBoolean.False,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
				Node.FromValue(NodeIdentifier.FromValue(4)),
				Port.FromValue(PortIdentifier.FromValue(5))),
			NetworkUserAddress.FromValue(SevenBitAsciiString.FromValue("MDT")),
			HoldTime.FromValue(30),
			ProtocolBoolean.True);
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = MobileDataTerminalTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("000200010105034D44541E01"));
		decodedEntry.NetworkUserAddress.Value.Value.Should().Be("MDT");
		decodedEntry.HoldTime.Value.Should().Be(30);
		decodedEntry.Available.Should().Be(ProtocolBoolean.True);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_wan_table_entry()
	{
		var entry = WanTableEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			ProtocolBoolean.False,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
				Node.FromValue(NodeIdentifier.FromValue(4)),
				Port.FromValue(PortIdentifier.FromValue(5))),
			WanAddress.FromValue(SevenBitAsciiString.FromValue("WAN")),
			ConnectType.FromValue(ConnectTypeValue.SwitchedVirtualCircuit));
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = WanTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("0002000101050357414E01"));
		decodedEntry.WanAddress.Value.Value.Should().Be("WAN");
		decodedEntry.ConnectType.Value.Should().Be(ConnectTypeValue.SwitchedVirtualCircuit);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_routing_table_entry()
	{
		var nextNode = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(1)),
			Node.FromValue(NodeIdentifier.FromValue(4)),
			Port.FromValue(PortIdentifier.FromValue(5)));
		var entry = RoutingTableEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			ProtocolBoolean.True,
			nextNode,
			DestinationNodes.FromAddressRanges(AddressRange.FromValues(nextNode, nextNode)),
			AgentType.FromValue(AgentTypeValue.NetworkManagementUserAgent),
			RoutingPreference.FromValue(0xC0));
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = RoutingTableEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("000201010105010101050101050CC0"));
		decodedEntry.DestinationNodes.AddressRanges.Should().HaveCount(1);
		decodedEntry.AgentType.Value.Should().Be(AgentTypeValue.NetworkManagementUserAgent);
		decodedEntry.Preference.Value.Should().Be(0xC0);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_telephone_statistics_entry()
	{
		var entry = TelephoneStatisticsEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			TimeAndDate.FromValue(SevenBitAsciiString.FromValue("01JAN25010203")),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("12")),
			ConnectTime.FromValue(0x1234));
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = TelephoneStatisticsEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("000230314A414E32353031303230330231321234"));
		decodedEntry.TimeAndDate.Value.Value.Should().Be("01JAN25010203");
		decodedEntry.TelephoneNumber.Value.Value.Should().Be("12");
		decodedEntry.ConnectTime.Value.Should().Be(0x1234);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_wan_statistics_entry()
	{
		var entry = WanStatisticsEntry.FromValues(
			ParameterEntryIndex.FromValue(2),
			TimeAndDate.FromValue(SevenBitAsciiString.FromValue("01JAN25010203")),
			WanAddress.FromValue(SevenBitAsciiString.FromValue("WAN")),
			ConnectTime.FromValue(0x1234));
		var encoded = entry.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var decodedEntry = WanStatisticsEntry.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("000230314A414E32353031303230330357414E1234"));
		decodedEntry.WanAddress.Value.Value.Should().Be("WAN");
		decodedEntry.ConnectTime.Value.Should().Be(0x1234);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
