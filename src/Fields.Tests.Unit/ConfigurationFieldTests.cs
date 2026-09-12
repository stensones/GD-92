using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ConfigurationFieldTests
{
	[Fact]
	public void Round_trips_an_active_when_contacts_open_state()
	{
		var encoded = ActiveState.FromValue(ActiveStateValue.ActiveWhenContactsOpen).ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var activeState = ActiveState.FromEncodedMessageBuffer(ref buffer);

		activeState.Value.Should().Be(ActiveStateValue.ActiveWhenContactsOpen);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Serializes_the_generate_alarm_on_both_and_regeneration_code()
	{
		GenerateAlarm.FromValue(GenerateAlarmValue.GenerateAlarmOnBothAndEachRegenerationPeriod)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x07 });
	}

	[Theory]
	[InlineData(ConnectTypeValue.PermanentVirtualCircuit, 0x00)]
	[InlineData(ConnectTypeValue.SwitchedVirtualCircuit, 0x01)]
	public void Serializes_each_connect_type(ConnectTypeValue value, byte expected)
	{
		ConnectType.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(DialTonesValue.PulseDialling, 0x00)]
	[InlineData(DialTonesValue.ToneDialling, 0x01)]
	public void Serializes_each_dial_tones_type(DialTonesValue value, byte expected)
	{
		DialTones.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(AgentTypeValue.IsdnBasicRate, 0x00)]
	[InlineData(AgentTypeValue.NetworkManagementUserAgent, 0x0C)]
	[InlineData(AgentTypeValue.UserDefined4, 0x19)]
	public void Serializes_defined_agent_types(AgentTypeValue value, byte expected)
	{
		AgentType.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Fact]
	public void Serializes_configuration_time_values_in_big_endian_order()
	{
		ConnectTime.FromValue(0x1234).ToWireValue().Should().Equal(Convert.FromHexString("1234"));
		PulseLength.FromValue(0x4567).ToWireValue().Should().Equal(Convert.FromHexString("4567"));
		RegenerationTime.FromValue(0x89AB).ToWireValue().Should().Equal(Convert.FromHexString("89AB"));
	}

	[Fact]
	public void Rejects_a_physical_bit_outside_the_defined_range()
	{
		var createPhysicalBit = () => PhysicalBit.FromValue(16);

		createPhysicalBit.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Serializes_a_routing_preference()
	{
		RoutingPreference.FromValue(0xC0).ToWireValue().Should().Equal(new byte[] { 0xC0 });
	}

	[Fact]
	public void Round_trips_a_connection_hold_time()
	{
		var encoded = HoldTime.FromValue(90).ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var holdTime = HoldTime.FromEncodedMessageBuffer(ref buffer);

		holdTime.Value.Should().Be(90);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Serializes_a_compressed_alternative_address_string()
	{
		AddressString.FromValue(SevenBitAsciiString.FromValue("AAAAA"))
			.ToWireValue()
			.Should()
			.Equal(Convert.FromHexString("031B4105"));
	}

	[Theory]
	[InlineData("LanAddress", "LAN", "034C414E")]
	[InlineData("WanAddress", "WAN", "0357414E")]
	[InlineData("NetworkUserAddress", "USER", "0455534552")]
	[InlineData("NodeName", "NODE", "044E4F4445")]
	public void Serializes_bounded_configuration_identifiers(
		string fieldName,
		string value,
		string expectedWireValue)
	{
		var wireValue = fieldName switch
		{
			"LanAddress" => LanAddress.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"WanAddress" => WanAddress.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"NetworkUserAddress" => NetworkUserAddress.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"NodeName" => NodeName.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};

		wireValue.Should().Equal(Convert.FromHexString(expectedWireValue));
	}

	[Fact]
	public void Rejects_a_network_user_address_longer_than_fourteen_characters()
	{
		var createNetworkUserAddress = () =>
			NetworkUserAddress.FromValue(SevenBitAsciiString.FromValue("ABCDEFGHIJKLMNO"));

		createNetworkUserAddress.Should().Throw<ArgumentOutOfRangeException>();
	}
}
