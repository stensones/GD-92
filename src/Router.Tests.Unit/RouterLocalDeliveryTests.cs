using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ParticipantParameters;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterLocalDeliveryTests
{
	[Fact]
	public async Task Delivers_a_Router_Parameter_Response_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress, 42));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress());

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				routerAddress,
				ParameterTable.Current),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue
			.ToWireValue().Should().Equal([42]);
	}

	[Fact]
	public async Task Delivers_a_requested_Routing_Table_entry_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var routingTable = RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable)));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = RoutingTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.NextNode.Should().Be(nextNode);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_NonVolatile_Routing_Table_entry_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var routingTable = RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable)));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = RoutingTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.NextNode.Should().Be(nextNode);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_Permanent_Routing_Table_entry_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var routingTable = RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable),
				parameterTable: ParameterTable.Permanent));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Permanent,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = RoutingTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.NextNode.Should().Be(nextNode);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_requested_PSTN_Table_entry_from_the_non_volatile_store()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var pstnTable = PstnTable.FromEntries(
			TelephoneTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("12")),
				HoldTime.FromValue(30),
				ProtocolBoolean.True));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.PstnTable.Encode(pstnTable),
				RouterParameterCatalogue.PstnTable.Number));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.PstnTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = PstnTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.Used.Should().Be(ProtocolBoolean.True);
		returnedEntry.NextNode.Should().Be(nextNode);
		returnedEntry.TelephoneNumber.Value.Value.Should().Be("12");
		returnedEntry.HoldTime.Value.Should().Be(30);
		returnedEntry.Available.Should().Be(ProtocolBoolean.True);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_requested_WAN_Table_entry_from_the_non_volatile_store()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var wanTable = WanTable.FromEntries(
			WanTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				WanAddress.FromValue(SevenBitAsciiString.FromValue("WAN")),
				ConnectType.FromValue(ConnectTypeValue.SwitchedVirtualCircuit)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			new RouterCurrentParameterProjectionSource(),
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.WanTable.Encode(wanTable),
				RouterParameterCatalogue.WanTable.Number));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.WanTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = WanTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.Used.Should().Be(ProtocolBoolean.True);
		returnedEntry.NextNode.Should().Be(nextNode);
		returnedEntry.WanAddress.Value.Value.Should().Be("WAN");
		returnedEntry.ConnectType.Value.Should().Be(ConnectTypeValue.SwitchedVirtualCircuit);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_requested_LAN_Table_entry_from_the_non_volatile_store()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var lanTable = LanTable.FromEntries(
			LanTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				LanAddress.FromValue(SevenBitAsciiString.FromValue("LAN"))));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			new RouterCurrentParameterProjectionSource(),
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.LanTable.Encode(lanTable),
				RouterParameterCatalogue.LanTable.Number));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.LanTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = LanTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.Used.Should().Be(ProtocolBoolean.True);
		returnedEntry.NextNode.Should().Be(nextNode);
		returnedEntry.LanAddress.Value.Value.Should().Be("LAN");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_requested_ISDN_Table_entry_from_the_non_volatile_store()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var isdnTable = IsdnTable.FromEntries(
			TelephoneTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("34")),
				HoldTime.FromValue(20),
				ProtocolBoolean.True));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			new RouterCurrentParameterProjectionSource(),
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.IsdnTable.Encode(isdnTable),
				RouterParameterCatalogue.IsdnTable.Number));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.IsdnTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var response = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		response.MoreValues.Should().Be(MoreValues.No);
		var buffer = new EncodedMessageBuffer(response.ParameterValue.ToWireValue());
		var returnedEntry = IsdnTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.Used.Should().Be(ProtocolBoolean.True);
		returnedEntry.NextNode.Should().Be(nextNode);
		returnedEntry.TelephoneNumber.Value.Value.Should().Be("34");
		returnedEntry.HoldTime.Value.Should().Be(20);
		returnedEntry.Available.Should().Be(ProtocolBoolean.True);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public async Task Delivers_a_requested_MDT_Table_entry_from_the_non_volatile_store()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var mdtTable = MdtTable.FromEntries(MobileDataTerminalTableEntry.FromValues(
			ParameterEntryIndex.FromValue(1), ProtocolBoolean.True, nextNode,
			NetworkUserAddress.FromValue(SevenBitAsciiString.FromValue("MDT")),
			HoldTime.FromValue(10), ProtocolBoolean.True));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress, new RouterCurrentParameterProjectionSource(), userAgentIngress,
			new CapturingLocalParticipantIngress(), new RetainedParameterStore(
				RouterParameterCatalogue.MdtTable.Encode(mdtTable),
				RouterParameterCatalogue.MdtTable.Number));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.MdtTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1), ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		var parameter = userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which;
		var buffer = new EncodedMessageBuffer(parameter.ParameterValue.ToWireValue());
		var returnedEntry = MdtTable.FromEncodedMessageBuffer(ref buffer).Entries.Single();
		returnedEntry.Index.Value.Should().Be(1);
		returnedEntry.NetworkUserAddress.Value.Value.Should().Be("MDT");
		returnedEntry.HoldTime.Value.Should().Be(10);
		returnedEntry.Available.Should().Be(ProtocolBoolean.True);
	}

	[Fact]
	public async Task Does_not_indicate_more_values_when_all_requested_Routing_Table_entries_fit()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var firstNextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var secondNextNode = RouterParameterModuleTestSupport.CreateAddress(26, 102, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var routingTable = RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				firstNextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)),
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(2),
				ProtocolBoolean.True,
				secondNextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable)));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(1))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.MoreValues
			.Should().Be(MoreValues.No);
	}

	[Fact]
	public async Task Returns_the_longest_Routing_Table_prefix_that_fits_in_a_Parameter_Response()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var routingTable = RoutingTable.FromEntries(
			[.. Enumerable.Range(1, 200).Select(index =>
				RoutingTableEntry.FromValues(
					ParameterEntryIndex.FromValue((ushort)index),
					ProtocolBoolean.True,
					RouterParameterModuleTestSupport.CreateAddress(26, (ushort)(100 + index), 0),
					DestinationNodes.FromAddressRanges(),
					AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
					RoutingPreference.FromValue(0)))]);
		var parameterRead = new RouterParameterRead(
			routerAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			parameterStore: new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable)));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(200))));

		var response = await parameterRead.HandleAsync(request, CancellationToken.None);

		response!.CountAndLength.MessageLength.Value.Should().BeLessThanOrEqualTo(
			(ushort)Envelope.MaximumContentsLength);
		var parameter = response.Contents.Should().BeOfType<Parameter>().Which;
		parameter.MoreValues.Should().Be(MoreValues.Yes);
		var buffer = new EncodedMessageBuffer(parameter.ParameterValue.ToWireValue());
		var returnedEntries = RoutingTable.FromEncodedMessageBuffer(ref buffer).Entries;
		returnedEntries.Should().NotBeEmpty();
		returnedEntries.Should().HaveCountLessThan(200);
		returnedEntries[0].Index.Value.Should().Be(1);
		buffer.RemainingBitCount.Should().Be(0);
		Parameter.FromFields(
			MoreValues.Yes,
			RouterParameterCatalogue.RouterTable.Encode(RoutingTable.FromEntries(
				[.. returnedEntries, routingTable.Entries[returnedEntries.Count]])))
			.ToWireValue().Length.Should().BeGreaterThan(Envelope.MaximumContentsLength);
	}

	[Fact]
	public async Task Rejects_a_Routing_Table_range_containing_a_missing_entry_with_a_parameter_Invalid_Entry_NAK()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nextNode = RouterParameterModuleTestSupport.CreateAddress(26, 101, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var routingTable = RoutingTable.FromEntries(
			RoutingTableEntry.FromValues(
				ParameterEntryIndex.FromValue(1),
				ProtocolBoolean.True,
				nextNode,
				DestinationNodes.FromAddressRanges(),
				AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
				RoutingPreference.FromValue(0)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			new RetainedParameterStore(
				RouterParameterCatalogue.RouterTable.Encode(routingTable)));
		var request = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.RouterTable.Number,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(1),
					ParameterEntryIndex.FromValue(2))));

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.InvalidEntry);
	}

	[Fact]
	public async Task Delivers_a_Node_Login_response_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var suppliedAddress = RouterParameterModuleTestSupport.CreateAddress(42, 200, 7);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress());

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateCurrentPasswordSet(
				routerAddress,
				PasswordLevelNumber.Level1,
				"FIRE",
				suppliedAddress),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().CurrentPassword.CommunicationsAddress.Should().Be(suppliedAddress);
	}

	[Fact]
	public async Task Delivers_a_Level1_Password_Modification_response_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress());

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				routerAddress,
				ParameterTable.Current,
				"WATER"),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))).Should().BeTrue();
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_Retries_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.Retries.Number,
					RouterParameterCatalogue.Retries.Encode(
						Retries.FromValue(Word8.FromValue(5))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Retries.Number)).Should().BeNull();
		currentParameters.GetCurrent().Retries.Value.Value.Should().Be(3);
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_Network_Manager_Address_1_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.NetworkManagerAddress1.Number,
					ParameterValue.FromWireValue([26, 25, 24]))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NetworkManagerAddress1.Number)).Should().BeNull();
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_source_matched_Level2_NonVolatile_Network_Manager_Address_1_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var networkManagerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 24);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level2,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.NetworkManagerAddress1.Number,
					ParameterValue.FromWireValue([26, 25, 24]))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		RouterParameterCatalogue.NetworkManagerAddress1.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.NetworkManagerAddress1.Number))!)
			.Should().Be(networkManagerAddress);
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_No_Acknowledgement_Timeout_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
						NoAcknowledgementTimeout.FromValue(Word8.FromValue(10))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Number)).Should().BeNull();
		currentParameters.GetCurrent().NoAcknowledgementTimeout.Value.Value.Should().Be(5);
	}

	[Fact]
	public async Task Delivers_an_acknowledged_Level2_NonVolatile_Maximum_Message_Length_change_without_changing_the_Current_startup_setting()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level2,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore,
			maximumMessageLength: MaximumMessageLength.FromValue(1023));

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.MaximumMessageLength.Number,
					RouterParameterCatalogue.MaximumMessageLength.Encode(
						MaximumMessageLength.FromValue(512)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.MaximumMessageLength.Number))!.ToWireValue().Should().Equal(
				[2, 0]);

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				routerAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.MaximumMessageLength.Number),
			CancellationToken.None);

		RouterParameterCatalogue.MaximumMessageLength.Read(
			userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue)
			.Value.Should().Be(1023);
	}

	[Fact]
	public async Task Delivers_an_acknowledged_Level2_Current_Maximum_Message_Length_change_without_changing_the_NonVolatile_value()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
		RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
		RouterParameterModuleTestSupport.CreatePasswordParameter(
			PasswordLevelNumber.Level2,
			"FIRE",
			userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		await parameterStore.StoreAsync(
		ParameterTable.NonVolatile,
		RouterParameterCatalogue.MaximumMessageLength.Number,
		RouterParameterCatalogue.MaximumMessageLength.Encode(
			MaximumMessageLength.FromValue(1023)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
		routerAddress,
		currentParameters,
		userAgentIngress,
		new CapturingLocalParticipantIngress(),
		parameterStore,
		maximumMessageLength: MaximumMessageLength.FromValue(1023));

		await localDelivery.ReceiveAsync(
		Envelope.FromValues(
			userAgentAddress,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.MaximumMessageLength.Number,
				RouterParameterCatalogue.MaximumMessageLength.Encode(
					MaximumMessageLength.FromValue(512)))),
		CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().MaximumMessageLength.Value.Should().Be(512);
		(await parameterStore.GetAsync(
		ParameterTable.NonVolatile,
		RouterParameterCatalogue.MaximumMessageLength.Number))!.ToWireValue().Should().Equal(
			[3, 255]);

		await localDelivery.ReceiveAsync(
		RouterParameterModuleTestSupport.CreateParameterRequest(
			routerAddress,
			ParameterTable.Current,
			RouterParameterCatalogue.MaximumMessageLength.Number),
		CancellationToken.None);

		RouterParameterCatalogue.MaximumMessageLength.Read(
		userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue)
		.Value.Should().Be(512);
	}

	[Fact]
	public async Task Delivers_an_Invalid_Syntax_rejection_for_a_malformed_Level2_NonVolatile_Maximum_Message_Length_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level2,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.MaximumMessageLength.Number,
					ParameterValue.FromWireValue([2, 0, 0]))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.InvalidSyntax);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.MaximumMessageLength.Number)).Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_Maximum_Message_Length_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.MaximumMessageLength.Number,
					RouterParameterCatalogue.MaximumMessageLength.Encode(
						MaximumMessageLength.FromValue(512)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.MaximumMessageLength.Number)).Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_Manual_Acknowledgement_Timeout_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
						ManualAcknowledgementTimeout.FromValue(30)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.ManualAcknowledgementTimeout.Number)).Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_NonVolatile_Brigade_or_Agency_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.BrigadeOrAgency.Number,
					RouterParameterCatalogue.BrigadeOrAgency.Encode(
						BrigadeOrAgencyIdentifier.FromValue(25)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.BrigadeOrAgency.Number)).Should().BeNull();
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_source_matched_Level3_NonVolatile_Brigade_or_Agency_change_without_changing_Current()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level3,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.BrigadeOrAgency.Number,
					RouterParameterCatalogue.BrigadeOrAgency.Encode(
						BrigadeOrAgencyIdentifier.FromValue(25)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		RouterParameterCatalogue.BrigadeOrAgency.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.BrigadeOrAgency.Number))!).Should().Be(
					BrigadeOrAgencyIdentifier.FromValue(25));
		currentParameters.GetCurrent().BrigadeOrAgencyIdentifier.Should().Be(
			BrigadeOrAgencyIdentifier.FromValue(26));
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_source_matched_Level3_Current_No_Acknowledgement_Timeout_change_without_changing_NonVolatile()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level3,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5))));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.Current,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
						NoAcknowledgementTimeout.FromValue(Word8.FromValue(10))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().NoAcknowledgementTimeout.Value.Value.Should().Be(10);
		RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number))!).Value.Value.Should().Be(5);
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_Level3_NonVolatile_No_Acknowledgement_Timeout_change_without_changing_Current()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level3,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
					RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
						NoAcknowledgementTimeout.FromValue(Word8.FromValue(10))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number))!).Value.Value.Should().Be(10);
		currentParameters.GetCurrent().NoAcknowledgementTimeout.Value.Value.Should().Be(5);
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_source_matched_Level3_NonVolatile_Manual_Acknowledgement_Timeout_change_without_changing_Current()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level3,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore,
			ManualAcknowledgementTimeout.FromValue(60));

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.NonVolatile,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
						ManualAcknowledgementTimeout.FromValue(30)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number))!).Value.Should().Be(
					30);

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				routerAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number),
			CancellationToken.None);

		RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
			userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue)
			.Value.Should().Be(60);
	}

	[Fact]
	public async Task Delivers_an_acknowledgement_for_a_source_matched_Level3_Current_Manual_Acknowledgement_Timeout_change_without_changing_NonVolatile()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOn(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level3,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
			RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
				ManualAcknowledgementTimeout.FromValue(60)));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore,
			ManualAcknowledgementTimeout.FromValue(60));

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.Current,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
						ManualAcknowledgementTimeout.FromValue(30)))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().ManualAcknowledgementTimeout.Value.Should().Be(30);
		RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
			(await parameterStore.GetAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number))!).Value.Should().Be(
					60);

		await localDelivery.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				routerAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number),
			CancellationToken.None);

		RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
			userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue)
			.Value.Should().Be(30);
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_a_Level1_Current_Retries_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.Current,
					RouterParameterCatalogue.Retries.Number,
					RouterParameterCatalogue.Retries.Encode(
						Retries.FromValue(Word8.FromValue(5))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Retries.Number)).Should().BeNull();
		currentParameters.GetCurrent().Retries.Value.Value.Should().Be(3);
	}

	[Fact]
	public async Task Delivers_a_no_modification_access_rejection_for_an_authorized_Permanent_Retries_change()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		currentParameters.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterStore = new RecordingParameterStore();
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress(),
			parameterStore);

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				userAgentAddress,
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.Permanent,
					RouterParameterCatalogue.Retries.Number,
					RouterParameterCatalogue.Retries.Encode(
						Retries.FromValue(Word8.FromValue(5))))),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Retries.Number)).Should().BeNull();
		currentParameters.GetCurrent().Retries.Value.Value.Should().Be(3);
	}

	[Fact]
	public async Task Does_not_deliver_an_unsupported_Router_Message()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			new CapturingLocalParticipantIngress());

		await localDelivery.ReceiveAsync(
			Envelope.FromValues(
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
				Destinations.FromAddresses(routerAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.NotRequested),
				Stensones.GD92.Messages.Text.FromFields(
					Block.FromValue(1),
					OfBlocks.FromValue(1),
					Stensones.GD92.Fields.Text.FromValue("FIRE"))),
			CancellationToken.None);

		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_management_Message_to_Local_Participant_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			localParticipantIngress);
		var request = RouterParameterModuleTestSupport.CreateParameterRequest(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 1),
			ParameterTable.Current);

		await localDelivery.ReceiveAsync(request, CancellationToken.None);

		localParticipantIngress.Envelope.Should().BeSameAs(request);
		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_each_established_local_management_Message_to_Local_Participant_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			localParticipantIngress);
		var participantAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 1);
		var setParameter = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(participantAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterValue.FromWireValue([1])));
		var parameterRequestMultiple = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
			Destinations.FromAddresses(participantAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(1))));

		await localDelivery.ReceiveAsync(setParameter, CancellationToken.None);
		await localDelivery.ReceiveAsync(parameterRequestMultiple, CancellationToken.None);

		localParticipantIngress.Envelopes.Should().Equal(setParameter, parameterRequestMultiple);
		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_non_management_Message_to_User_Agent_Ingress()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			localParticipantIngress);
		var message = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 1),
			Destinations.FromAddresses(RouterParameterModuleTestSupport.CreateAddress(26, 100, 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Stensones.GD92.Messages.Text.FromFields(
				Block.FromValue(1),
				OfBlocks.FromValue(1),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));

		await localDelivery.ReceiveAsync(message, CancellationToken.None);

		userAgentIngress.Envelope.Should().BeSameAs(message);
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Does_not_deliver_a_non_local_or_multi_destination_Message(
		bool isMultiDestination)
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress));
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var localDelivery = CreateLocalDelivery(
			routerAddress,
			currentParameters,
			userAgentIngress,
			localParticipantIngress);
		var destinations = isMultiDestination
			? Destinations.FromAddresses(
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 26))
			: Destinations.FromAddresses(
				RouterParameterModuleTestSupport.CreateAddress(42, 100, 25));
		var message = Envelope.FromValues(
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 1),
			destinations,
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Stensones.GD92.Messages.Text.FromFields(
				Block.FromValue(1),
				OfBlocks.FromValue(1),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));

		await localDelivery.ReceiveAsync(message, CancellationToken.None);

		userAgentIngress.Envelope.Should().BeNull();
		localParticipantIngress.Envelope.Should().BeNull();
	}

	private static RouterLocalDelivery CreateLocalDelivery(
		CommunicationsAddress routerAddress,
		RouterCurrentParameterProjectionSource currentParameters,
		IUserAgentIngress userAgentIngress,
		ILocalParticipantIngress localParticipantIngress,
		IParticipantParameterStore? parameterStore = null,
		ManualAcknowledgementTimeout? manualAcknowledgementTimeout = null,
		MaximumMessageLength? maximumMessageLength = null) =>
		new(
			routerAddress,
			new RouterParameterRead(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters,
				parameterStore,
				maximumMessageLength: maximumMessageLength,
				manualAcknowledgementTimeout: manualAcknowledgementTimeout),
			new NodeLogin(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters),
			new Level1PasswordModification(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters,
				new InMemoryPasswordVerifierStore()),
			new RouterParameterModification(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters,
				parameterStore ?? new RetainedParameterStore(ParameterValue.FromWireValue([]))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterLocalDelivery>.Instance);

	private sealed class RetainedParameterStore(
		ParameterValue parameterValue,
		ParameterNumber? retainedParameterNumber = null,
		ParameterTable? parameterTable = null) :
		IParticipantParameterStore
	{
		private readonly ParameterTable storedTable = parameterTable ?? ParameterTable.NonVolatile;

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default) =>
			ValueTask.FromResult<ParameterValue?>(
				parameterTable == this.storedTable &&
				parameterNumber == (retainedParameterNumber ?? RouterParameterCatalogue.RouterTable.Number)
					? parameterValue
					: null);

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException();

		public ValueTask<T> ExecuteInitializationAsync<T>(
			Func<CancellationToken, ValueTask<T>> initialize,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException();
	}

	private sealed class RecordingParameterStore : IParticipantParameterStore
	{
		private readonly Dictionary<(ParameterTable Table, ParameterNumber Number), ParameterValue>
			values = [];

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default) =>
			ValueTask.FromResult<ParameterValue?>(
				this.values.GetValueOrDefault((parameterTable, parameterNumber)));

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			this.values[(parameterTable, parameterNumber)] = parameterValue;
			return ValueTask.CompletedTask;
		}

		public ValueTask<T> ExecuteInitializationAsync<T>(
			Func<CancellationToken, ValueTask<T>> initialize,
			CancellationToken cancellationToken = default) =>
			initialize(cancellationToken);
	}

	private sealed class CapturingUserAgentIngress : IUserAgentIngress
	{
		public Envelope? Envelope { get; private set; }

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class CapturingLocalParticipantIngress : ILocalParticipantIngress
	{
		public List<Envelope> Envelopes { get; } = [];
		public Envelope? Envelope => this.Envelopes.LastOrDefault();

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelopes.Add(envelope);
			return Task.CompletedTask;
		}
	}
}
