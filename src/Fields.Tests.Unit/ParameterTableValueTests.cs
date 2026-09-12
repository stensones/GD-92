using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterTableValueTests
{
	[Fact]
	public void Round_trips_an_address_table_with_multiple_entries()
	{
		var entry = AlternativeAddressTableEntry.FromValues(
			AddressRange.FromValues(
				CommunicationsAddress.FromValues(
					Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
					Node.FromValue(NodeIdentifier.FromValue(0)),
					Port.FromValue(PortIdentifier.FromValue(0))),
				CommunicationsAddress.FromValues(
					Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
					Node.FromValue(NodeIdentifier.FromValue(0)),
					Port.FromValue(PortIdentifier.FromValue(1)))),
			AddressString.FromValue(SevenBitAsciiString.FromValue("A")));
		var encoded = AddressTable.FromEntries(entry, entry).ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var addressTable = AddressTable.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("00000000000101410000000000010141"));
		addressTable.Entries.Should().HaveCount(2);
		addressTable.Entries[1].AddressString.Value.Value.Should().Be("A");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_a_lan_table_with_multiple_entries()
	{
		var encoded = LanTable.FromEntries(
			CreateLanTableEntry(1, ProtocolBoolean.True, 0, "A"),
			CreateLanTableEntry(2, ProtocolBoolean.False, 1, "B"))
			.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var lanTable = LanTable.FromEncodedMessageBuffer(ref buffer);

		encoded.Should().Equal(Convert.FromHexString("00010100000001410002000000010142"));
		lanTable.Entries.Should().HaveCount(2);
		lanTable.Entries[0].Used.Should().Be(ProtocolBoolean.True);
		lanTable.Entries[1].NextNode.Port.Value.Should().Be(1);
		lanTable.Entries[1].LanAddress.Value.Value.Should().Be("B");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_pstn_and_isdn_tables()
	{
		var entry = TelephoneTableEntry.FromValues(
			ParameterEntryIndex.FromValue(1),
			ProtocolBoolean.False,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
				Node.FromValue(NodeIdentifier.FromValue(0)),
				Port.FromValue(PortIdentifier.FromValue(0))),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("1")),
			HoldTime.FromValue(0),
			ProtocolBoolean.False);
		var pstnBuffer = new EncodedMessageBuffer(PstnTable.FromEntries(entry).ToWireValue());
		var isdnBuffer = new EncodedMessageBuffer(IsdnTable.FromEntries(entry).ToWireValue());

		var pstnTable = PstnTable.FromEncodedMessageBuffer(ref pstnBuffer);
		var isdnTable = IsdnTable.FromEncodedMessageBuffer(ref isdnBuffer);

		pstnTable.Entries.Single().TelephoneNumber.Value.Value.Should().Be("1");
		isdnTable.Entries.Single().TelephoneNumber.Value.Value.Should().Be("1");
		pstnBuffer.RemainingBitCount.Should().Be(0);
		isdnBuffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_the_remaining_table_value_types()
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
			Node.FromValue(NodeIdentifier.FromValue(0)),
			Port.FromValue(PortIdentifier.FromValue(0)));
		var mdtTableBuffer = new EncodedMessageBuffer(MdtTable.FromEntries(
			MobileDataTerminalTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.False,
				address,
				NetworkUserAddress.FromValue(SevenBitAsciiString.FromValue("M")),
				HoldTime.FromValue(0),
				ProtocolBoolean.False)).ToWireValue());
		var wanTableBuffer = new EncodedMessageBuffer(WanTable.FromEntries(
			WanTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.False,
				address,
				WanAddress.FromValue(SevenBitAsciiString.FromValue("W")),
				ConnectType.FromValue(ConnectTypeValue.PermanentVirtualCircuit))).ToWireValue());
		var routingTableBuffer = new EncodedMessageBuffer(RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.False,
				address,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.Printer),
				RoutingPreference.FromValue(0))).ToWireValue());
		var telephoneStatisticsBuffer = new EncodedMessageBuffer(TelephoneStatistics.FromEntries(
			TelephoneStatisticsEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				TimeAndDate.FromValue(SevenBitAsciiString.FromValue("01JAN25010203")),
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("1")),
				ConnectTime.FromValue(0))).ToWireValue());
		var wanStatisticsBuffer = new EncodedMessageBuffer(WanStatistics.FromEntries(
			WanStatisticsEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				TimeAndDate.FromValue(SevenBitAsciiString.FromValue("01JAN25010203")),
				WanAddress.FromValue(SevenBitAsciiString.FromValue("W")),
				ConnectTime.FromValue(0))).ToWireValue());

		MdtTable.FromEncodedMessageBuffer(ref mdtTableBuffer).Entries.Single().NetworkUserAddress.Value.Value.Should().Be("M");
		WanTable.FromEncodedMessageBuffer(ref wanTableBuffer).Entries.Single().WanAddress.Value.Value.Should().Be("W");
		RoutingTable.FromEncodedMessageBuffer(ref routingTableBuffer).Entries.Single().AgentType.Value.Should().Be(AgentTypeValue.Printer);
		TelephoneStatistics.FromEncodedMessageBuffer(ref telephoneStatisticsBuffer).Entries.Single().TelephoneNumber.Value.Value.Should().Be("1");
		WanStatistics.FromEncodedMessageBuffer(ref wanStatisticsBuffer).Entries.Single().WanAddress.Value.Value.Should().Be("W");
		mdtTableBuffer.RemainingBitCount.Should().Be(0);
		wanTableBuffer.RemainingBitCount.Should().Be(0);
		routingTableBuffer.RemainingBitCount.Should().Be(0);
		telephoneStatisticsBuffer.RemainingBitCount.Should().Be(0);
		wanStatisticsBuffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Supports_an_empty_table_and_rejects_more_than_five_address_entries()
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
			Node.FromValue(NodeIdentifier.FromValue(0)),
			Port.FromValue(PortIdentifier.FromValue(0)));
		var entry = AlternativeAddressTableEntry.FromValues(
			AddressRange.FromValues(address, address),
			AddressString.FromValue(SevenBitAsciiString.FromValue("A")));
		var emptyTableBuffer = new EncodedMessageBuffer(AddressTable.FromEntries().ToWireValue());
		var createAddressTable = () =>
			AddressTable.FromEntries(Enumerable.Repeat(entry, 6).ToArray());

		AddressTable.FromEncodedMessageBuffer(ref emptyTableBuffer).Entries.Should().BeEmpty();
		createAddressTable.Should().Throw<ArgumentOutOfRangeException>();
		emptyTableBuffer.RemainingBitCount.Should().Be(0);
	}

	private static LanTableEntry CreateLanTableEntry(
		ushort index,
		ProtocolBoolean used,
		byte port,
		string lanAddress)
	{
		return LanTableEntry.FromValues(
			ParameterEntryIndex.FromValue(index),
			used,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(0)),
				Node.FromValue(NodeIdentifier.FromValue(0)),
				Port.FromValue(PortIdentifier.FromValue(port))),
			LanAddress.FromValue(SevenBitAsciiString.FromValue(lanAddress)));
	}
}
