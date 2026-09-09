using AwesomeAssertions;
using Router;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Tests.Unit;

public sealed class LocalDeliveryClassificationTests
{
	[Fact]
	public void Rejects_missing_local_Router_configuration()
	{
		var createClassification = () => new LocalDeliveryClassification(null!);

		createClassification.Should().Throw<ArgumentNullException>()
			.Which.ParamName.Should().Be("localRouter");
	}

	[Fact]
	public void Classifies_a_non_local_Envelope_as_not_locally_deliverable()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 101, port: 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.NotLocallyDeliverable);
	}

	[Fact]
	public void Classifies_a_multi_destination_Envelope_as_not_locally_deliverable()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(
				Address(brigade: 26, node: 100, port: 1),
				Address(brigade: 26, node: 100, port: 2)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.NotLocallyDeliverable);
	}

	[Fact]
	public void Classifies_a_single_local_Text_Message_for_User_Agent_Ingress()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 1),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Stensones.GD92.Messages.Text.FromFields(
				Block.FromValue(1),
				OfBlocks.FromValue(1),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.UserAgentIngress);
	}

	[Fact]
	public void Classifies_a_single_local_Set_Parameter_for_Local_Participant_Ingress()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterValue.FromWireValue([1])));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.LocalParticipantIngress);
	}

	[Fact]
	public void Classifies_a_single_local_Parameter_Request_Multiple_for_Local_Participant_Ingress()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(1))));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.LocalParticipantIngress);
	}

	[Fact]
	public void Classifies_a_single_local_Parameter_Request_for_Local_Participant_Ingress()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.LocalParticipantIngress);
	}

	[Fact]
	public void Classifies_a_single_Envelope_addressed_to_the_local_Router_for_Router_handling()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(localRouter),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));
		var classification = new LocalDeliveryClassification(localRouter);

		var outcome = classification.Classify(envelope);

		outcome.Should().Be(LocalDeliveryOutcome.RouterHandling);
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
