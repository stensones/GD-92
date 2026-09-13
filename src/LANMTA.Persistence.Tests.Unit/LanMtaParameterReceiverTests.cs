using AwesomeAssertions;
using LANMTA;
using LANMTA.Persistence;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace LANMTA.Persistence.Tests.Unit;

public sealed class LanMtaParameterReceiverTests
{
	[Fact]
	public async Task Returns_a_retained_non_volatile_parameter()
	{
		var localAddress = Address(1);
		var source = Address(25);
		var routerIngress = new RecordingRouterIngress();
		var receiver = new LanMtaParameterReceiver(
			new LanMtaSettings(localAddress, Address(0), ProtocolVersion.FromValue(
				ProtocolVersionNumber.FromValue(2))),
			new LanMtaCurrentParameterProjectionSource(),
			new RetainedParameterReader(),
			routerIngress);

		var request = Envelope.FromValues(
			source,
			Destinations.FromAddresses(localAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.NonVolatile,
				ParameterNumber.FromValue(1)));
		var buffer = new EncodedMessageBuffer(request.ToWireValue());

		await receiver.ReceiveAsync(
			Envelope.FromEncodedMessageBuffer(ref buffer),
			CancellationToken.None);

		routerIngress.Submitted!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue
			.ToWireValue().Should().Equal([1]);
	}

	[Fact]
	public async Task Returns_a_retained_permanent_parameter()
	{
		var localAddress = Address(1);
		var source = Address(25);
		var routerIngress = new RecordingRouterIngress();
		var receiver = new LanMtaParameterReceiver(
			new LanMtaSettings(localAddress, Address(0), ProtocolVersion.FromValue(
				ProtocolVersionNumber.FromValue(2))),
			new LanMtaCurrentParameterProjectionSource(),
			new RetainedParameterReader(),
			routerIngress);

		var request = Envelope.FromValues(
			source,
			Destinations.FromAddresses(localAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Permanent,
				ParameterNumber.FromValue(1)));
		var buffer = new EncodedMessageBuffer(request.ToWireValue());

		await receiver.ReceiveAsync(
			Envelope.FromEncodedMessageBuffer(ref buffer),
			CancellationToken.None);

		routerIngress.Submitted!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue
			.ToWireValue().Should().Equal([1]);
	}

	private static CommunicationsAddress Address(byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class RetainedParameterReader : ILanMtaRetainedParameterReader
	{
		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			return ValueTask.FromResult<ParameterValue?>(
				(parameterTable == ParameterTable.NonVolatile ||
					parameterTable == ParameterTable.Permanent) &&
				parameterNumber == ParameterNumber.FromValue(1)
					? ParameterValue.FromWireValue([1])
					: null);
		}
	}

	private sealed class RecordingRouterIngress : IRouterIngress
	{
		public Envelope? Submitted { get; private set; }

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Submitted = envelope;
			return Task.CompletedTask;
		}
	}
}
