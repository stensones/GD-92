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
	public async Task Rejects_a_missing_Routing_Table_entry_with_a_parameter_Invalid_Entry_NAK()
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
					ParameterEntryIndex.FromValue(2),
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
		IParticipantParameterStore? parameterStore = null) =>
		new(
			routerAddress,
			new RouterParameterRead(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters,
				parameterStore),
			new NodeLogin(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters),
			new Level1PasswordModification(
				routerAddress,
				RouterParameterModuleTestSupport.ProtocolVersion,
				currentParameters,
				new InMemoryPasswordVerifierStore()),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterLocalDelivery>.Instance);

	private sealed class RetainedParameterStore(ParameterValue routingTable) :
		IParticipantParameterStore
	{
		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default) =>
			ValueTask.FromResult<ParameterValue?>(
				parameterTable == ParameterTable.NonVolatile &&
				parameterNumber == RouterParameterCatalogue.RouterTable.Number
					? routingTable
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
