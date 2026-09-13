using AwesomeAssertions;
using PrinterUA;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace PrinterUA.Persistence.Tests.Unit;

public sealed class PrinterUaTextMessageReceiverTests
{
	[Fact]
	public async Task Prints_a_received_text_message_and_acknowledges_the_source()
	{
		var printerUaAddress = Address(26, 100, 2);
		var source = Address(26, 100, 25);
		var printer = new RecordingPrinter();
		var routerIngress = new RecordingRouterIngress();
		var receiver = new PrinterUaTextMessageReceiver(
			new PrinterUaSettings(
				printerUaAddress,
				Address(26, 100, 0),
				source,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			printer,
			routerIngress);

		await receiver.ReceiveAsync(
			Envelope.FromValues(
				source,
				Destinations.FromAddresses(printerUaAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)),
					AcknowledgementRequest.Requested),
				Stensones.GD92.Messages.Text.FromFields(
					Block.FromValue(1),
					OfBlocks.FromValue(1),
					Stensones.GD92.Fields.Text.FromValue("Station message"))),
			CancellationToken.None);

		printer.PrintedText.Should().Be("Station message");
		routerIngress.Submitted.Should().NotBeNull();
		routerIngress.Submitted!.Contents.Should().BeOfType<Acknowledgement>();
		routerIngress.Submitted.Source.Should().Be(printerUaAddress);
		routerIngress.Submitted.Destinations.Addresses.Should().Equal(source);
	}

	[Fact]
	public async Task Rejects_a_received_text_message_when_the_printer_is_offline()
	{
		var printerUaAddress = Address(26, 100, 2);
		var source = Address(26, 100, 25);
		var routerIngress = new RecordingRouterIngress();
		var receiver = new PrinterUaTextMessageReceiver(
			new PrinterUaSettings(
				printerUaAddress,
				Address(26, 100, 0),
				source,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			new OfflinePrinter(),
			routerIngress);

		await receiver.ReceiveAsync(
			Envelope.FromValues(
				source,
				Destinations.FromAddresses(printerUaAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)),
					AcknowledgementRequest.Requested),
				Stensones.GD92.Messages.Text.FromFields(
					Block.FromValue(1),
					OfBlocks.FromValue(1),
					Stensones.GD92.Fields.Text.FromValue("Station message"))),
			CancellationToken.None);

		var negativeAcknowledgement = routerIngress.Submitted!.Contents
			.Should().BeOfType<NegativeAcknowledgement>().Subject;
		negativeAcknowledgement.ReasonCode.PrinterReasonCode.Should().Be(PrinterReasonCode.OffLine);
		routerIngress.Submitted.Source.Should().Be(printerUaAddress);
		routerIngress.Submitted.Destinations.Addresses.Should().Equal(source);
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class RecordingPrinter : ITextPrinter
	{
		public string? PrintedText { get; private set; }

		public Task PrintAsync(string text, CancellationToken cancellationToken)
		{
			this.PrintedText = text;
			return Task.CompletedTask;
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

	private sealed class OfflinePrinter : ITextPrinter
	{
		public Task PrintAsync(string text, CancellationToken cancellationToken)
		{
			throw new PrinterUnavailableException(PrinterReasonCode.OffLine);
		}
	}
}
