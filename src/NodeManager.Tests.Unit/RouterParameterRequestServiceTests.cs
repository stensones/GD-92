using AwesomeAssertions;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit;

public sealed class RouterParameterRequestServiceTests
{
	[Fact]
	public async Task Submits_the_local_Router_brigade_request_as_a_Management_Transaction()
	{
		var transactions = new RecordingManagementTransactions();
		var service = new RouterParameterRequestService(Settings(), transactions);

		await service.RequestLocalRouterBrigadeOrAgencyNumber(CancellationToken.None);

		transactions.Request!.Kind.Should().Be(ManagementTransactionKind.ParameterRequest);
		var envelope = transactions.Envelope!;
		envelope.Contents.Should().BeOfType<ParameterRequest>().Which.ParameterNumber.Value.Should().Be(1);
		envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested.Should().BeTrue();
	}

	[Fact]
	public async Task Submits_a_NonVolatile_Retries_change_as_a_Parameter_Modification()
	{
		var transactions = new RecordingManagementTransactions();
		var service = new RouterParameterRequestService(Settings(), transactions);

		await service.ModifyLocalRouterParameter(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(19),
			ParameterValue.FromWireValue([5]),
			CancellationToken.None);

		transactions.Request!.Kind.Should().Be(ManagementTransactionKind.ParameterModification);
		var setParameter = transactions.Envelope!.Contents.Should().BeOfType<SetParameter>().Which;
		setParameter.ParameterTable.Should().Be(ParameterTable.NonVolatile);
		setParameter.ParameterNumber.Value.Should().Be(19);
		setParameter.ParameterValue.ToWireValue().Should().Equal([5]);
		transactions.Envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested.Should().BeTrue();
	}

	[Fact]
	public async Task Submits_a_Node_Login_with_the_supplied_User_Agent_address()
	{
		var transactions = new RecordingManagementTransactions();
		var service = new RouterParameterRequestService(Settings(), transactions);
		var userAgent = Address(25);

		await service.RequestLocalRouterLogon(
			userAgent,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1")),
			CancellationToken.None);

		transactions.Request!.Kind.Should().Be(ManagementTransactionKind.NodeLogin);
		transactions.Request.NodeLoginUserAgentAddress.Should().Be(userAgent);
		transactions.Envelope!.Contents.Should().BeOfType<SetParameter>().Which.ParameterNumber.Value.Should().Be(4);
	}

	[Fact]
	public async Task Submits_a_Node_Logoff_through_the_common_transaction_lifecycle()
	{
		var transactions = new RecordingManagementTransactions();
		var service = new RouterParameterRequestService(Settings(), transactions);

		await service.RequestLocalRouterLogoff(CancellationToken.None);

		transactions.Request!.Kind.Should().Be(ManagementTransactionKind.NodeLogoff);
		var logoff = transactions.Envelope!.Contents.Should().BeOfType<SetParameter>().Which;
		var passwordBuffer = new EncodedMessageBuffer(logoff.ParameterValue.ToWireValue());
		PasswordParameter.FromEncodedMessageBuffer(ref passwordBuffer).Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated));
	}

	private static RouterParameterRequestSettings Settings() =>
		new(Address(25), Address(0));

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class RecordingManagementTransactions : IManagementTransactionService
	{
		public ManagementTransactionRequest? Request { get; private set; }
		public Envelope? Envelope { get; private set; }

		public Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
			ManagementTransactionRequest request,
			CancellationToken cancellationToken)
		{
			this.Request = request;
			this.Envelope = request.CreateEnvelope(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)));
			return Task.FromResult(new RouterParameterRequestStatusIdentifier(
				new UniqueSystemWideReference(
					request.Source,
					request.Destination,
					this.Envelope.AcknowledgementAndSequence.SequenceNumber)));
		}

		public Task<RouterParameterRequestStatus> WaitForCompletionAsync(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			CancellationToken cancellationToken) =>
			throw new NotSupportedException();
	}
}
