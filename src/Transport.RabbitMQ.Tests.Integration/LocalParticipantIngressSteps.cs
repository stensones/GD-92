using System.Reflection;
using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Integration;

[Binding]
public sealed class LocalParticipantIngressSteps
{
	private ILocalParticipantIngress? ingress;
	private Envelope? envelope;
	private Func<Task>? deliver;

	[Given(@"a local Router has a Local Participant Ingress")]
	public void GivenALocalRouterHasALocalParticipantIngress()
	{
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		this.ingress = new RabbitMqLocalParticipantIngress(messageBus);
	}

	[When(@"it delivers a management Envelope addressed to two local participants")]
	public void WhenItDeliversAManagementEnvelopeAddressedToTwoLocalParticipants()
	{
		this.envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
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
		this.deliver = () => this.ingress!.DeliverAsync(this.envelope, CancellationToken.None);
	}

	[Then(@"Local Participant Ingress rejects the Envelope before selecting an endpoint")]
	public async Task ThenLocalParticipantIngressRejectsTheEnvelopeBeforeSelectingAnEndpoint()
	{
		await this.deliver.Should().ThrowAsync<ArgumentException>();
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private class RecordingMessageBus : DispatchProxy
	{
		protected override object? Invoke(MethodInfo? targetMethod, object?[]? arguments)
		{
			throw new Xunit.Sdk.XunitException("Local Participant Ingress selected an endpoint.");
		}
	}
}
