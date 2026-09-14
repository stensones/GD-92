using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterParameterReadTests
{
	[Fact]
	public async Task Returns_the_Current_Brigade_or_Agency_Parameter_from_the_published_projection()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress, 42));
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal([42]);
	}

	[Fact]
	public async Task Returns_the_Current_Node_Number_Parameter_from_the_local_address()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.NodeNumber.Number),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal([0, 100]);
	}

	[Fact]
	public async Task Returns_the_Current_Node_Name_Parameter_from_the_configured_Node_Name()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var nodeName = NodeName.FromValue(SevenBitAsciiString.FromValue("Station End"));
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			nodeName: nodeName);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.NodeName.Number),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal(nodeName.ToWireValue());
	}

	[Fact]
	public async Task Returns_the_Current_Maximum_Message_Length_Parameter_from_the_configuration()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var maximumMessageLength = MaximumMessageLength.FromValue(1023);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			maximumMessageLength: maximumMessageLength);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.MaximumMessageLength.Number),
			CancellationToken.None);

		RouterParameterCatalogue.MaximumMessageLength.Read(
			response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue).Value
			.Should().Be(1023);
	}

	[Fact]
	public async Task Returns_the_Current_Network_Manager_Address_1_Parameter_from_the_configuration()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var networkManagerAddress1 = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			networkManagerAddress1: networkManagerAddress1);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.NetworkManagerAddress1.Number),
			CancellationToken.None);

		RouterParameterCatalogue.NetworkManagerAddress1.Read(
			response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue)
			.Should().Be(networkManagerAddress1);
	}

	[Fact]
	public async Task Leaves_an_unsupported_Router_Parameter_unhandled()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Permanent),
			CancellationToken.None);

		response.Should().BeNull();
	}

	[Fact]
	public async Task Rejects_an_unsupported_Current_Router_Parameter_with_a_parameter_Invalid_Parameter_NAK()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				ParameterNumber.FromValue(99)),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which
			.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.InvalidParameter);
	}

	[Fact]
	public async Task Returns_each_password_parameter_as_a_redacted_Password()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			new RouterCurrentParameterProjectionSource());
		foreach (var number in RouterParameterCatalogue.PasswordNumbers)
		{
			var request = Envelope.FromValues(
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25),
				Destinations.FromAddresses(localAddress),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					RouterParameterModuleTestSupport.ProtocolVersion),
				AcknowledgementAndSequence.FromValues(
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
					AcknowledgementRequest.Requested),
				ParameterRequest.FromFields(ParameterTable.Current, number));

			var response = await parameterRead.HandleAsync(request, CancellationToken.None);

			var buffer = new EncodedMessageBuffer(
				response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue());
			var password = Password.FromEncodedMessageBuffer(ref buffer);
			password.Value.Value.Value.Should().Be("PASSWORD");
		}
	}

	[Fact]
	public async Task Returns_the_Current_Password_with_its_Password_redacted()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var userAgentAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		source.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				userAgentAddress)).Should().BeTrue();
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);
		var request = Envelope.FromValues(
			userAgentAddress,
			Destinations.FromAddresses(localAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				RouterParameterModuleTestSupport.ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.CurrentPassword.Number));

		var response = await parameterRead.HandleAsync(request, CancellationToken.None);

		var buffer = new EncodedMessageBuffer(
			response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue());
		var password = PasswordParameter.FromEncodedMessageBuffer(ref buffer);
		password.Level.Should().Be(PasswordLevel.FromValue(PasswordLevelNumber.Level1));
		password.CommunicationsAddress.Should().Be(userAgentAddress);
		password.Password.Value.Value.Value.Should().Be("PASSWORD");
	}

	[Fact]
	public async Task Returns_the_Current_No_Acknowledgement_Timeout_from_the_published_projection()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal([5]);
	}

	[Fact]
	public async Task Returns_the_Current_Retries_from_the_published_projection()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		var parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var response = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current,
				RouterParameterCatalogue.Retries.Number),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal([3]);
	}
}

public sealed class NodeLoginTests
{
	[Fact]
	public async Task Rejects_an_invalid_Level1_Current_Password_with_the_existing_NAK()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		var nodeLogin = new NodeLogin(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var response = await nodeLogin.HandleAsync(
			RouterParameterModuleTestSupport.CreateCurrentPasswordSet(
				localAddress,
				PasswordLevelNumber.Level1,
				"WATER",
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25)),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<NegativeAcknowledgement>()
			.Which.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.InvalidPassword);
	}

	[Fact]
	public async Task Acknowledges_Level0_Current_Password_and_clears_the_active_Node_Login()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		source.TryLogOnAtLevelOne(RouterParameterModuleTestSupport.CreatePasswordParameter(
			PasswordLevelNumber.Level1,
			"FIRE",
			RouterParameterModuleTestSupport.CreateAddress(26, 100, 25))).Should().BeTrue();
		var nodeLogin = new NodeLogin(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var response = await nodeLogin.HandleAsync(
			RouterParameterModuleTestSupport.CreateCurrentPasswordSet(
				localAddress,
				PasswordLevelNumber.Unauthenticated,
				"ignored",
				RouterParameterModuleTestSupport.CreateAddress(42, 200, 7)),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Acknowledgement>();
		source.GetCurrent().CurrentPassword.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated));
		source.GetCurrent().CurrentPassword.CommunicationsAddress.Should().Be(localAddress);
		source.GetCurrent().CurrentPassword.Password.Value.Value.Value.Should().BeEmpty();
	}
}

public sealed class Level1PasswordModificationTests
{
	[Fact]
	public async Task Changes_the_Current_Level1_Password_without_durable_storage()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var requestSource = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		source.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				requestSource)).Should().BeTrue();
		var store = new InMemoryPasswordVerifierStore();
		var modification = new Level1PasswordModification(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source,
			store);

		var response = await modification.HandleAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				localAddress,
				ParameterTable.Current,
				"WATER"),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<Acknowledgement>();
		source.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))).Should().BeTrue();
		store.StoredValues.Should().BeEmpty();
	}

	[Fact]
	public async Task Acknowledges_a_NonVolatile_change_only_after_storing_its_verifier()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var requestSource = RouterParameterModuleTestSupport.CreateAddress(26, 100, 25);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		source.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				requestSource)).Should().BeTrue();
		var store = new BlockingPasswordVerifierStore();
		var modification = new Level1PasswordModification(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source,
			store);

		var handling = modification.HandleAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				localAddress,
				ParameterTable.NonVolatile,
				"WATER"),
			CancellationToken.None).AsTask();
		await store.StoreStarted.Task;

		handling.IsCompleted.Should().BeFalse();
		store.CompleteStore();
		var response = await handling;

		response!.Contents.Should().BeOfType<Acknowledgement>();
		store.StoredValues.Should().ContainKey((
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber));
	}

	[Fact]
	public async Task Rejects_a_Permanent_change_with_the_existing_No_Modification_Access_NAK()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(RouterParameterModuleTestSupport.CreateCurrentParameters(localAddress));
		source.TryLogOnAtLevelOne(
			RouterParameterModuleTestSupport.CreatePasswordParameter(
				PasswordLevelNumber.Level1,
				"FIRE",
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25))).Should().BeTrue();
		var modification = new Level1PasswordModification(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source,
			new InMemoryPasswordVerifierStore());

		var response = await modification.HandleAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				localAddress,
				ParameterTable.Permanent,
				"WATER"),
			CancellationToken.None);

		response!.Contents.Should().BeOfType<NegativeAcknowledgement>()
			.Which.ReasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.NoModificationAccess);
	}
}

internal static class RouterParameterModuleTestSupport
{
	public static ProtocolVersion ProtocolVersion { get; } =
		Stensones.GD92.Fields.ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2));

	public static Envelope CreateParameterRequest(
		CommunicationsAddress destination,
		ParameterTable parameterTable)
	{
		return CreateParameterRequest(
			destination,
			parameterTable,
			RouterParameterCatalogue.BrigadeOrAgency.Number);
	}

	public static Envelope CreateParameterRequest(
		CommunicationsAddress destination,
		ParameterTable parameterTable,
		ParameterNumber parameterNumber)
	{
		return Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				parameterTable,
				parameterNumber));
	}

	public static Envelope CreateCurrentPasswordSet(
		CommunicationsAddress destination,
		PasswordLevelNumber passwordLevel,
		string password,
		CommunicationsAddress suppliedAddress)
	{
		return Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.CurrentPassword.Number,
				ParameterValue.FromWireValue(CreatePasswordParameter(
					passwordLevel,
					password,
					suppliedAddress).ToWireValue())));
	}

	public static Envelope CreateLevel1PasswordChange(
		CommunicationsAddress destination,
		ParameterTable parameterTable,
		string password)
	{
		return Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				parameterTable,
				RouterParameterCatalogue.Level1PasswordNumber,
				ParameterValue.FromWireValue(
					Password.FromValue(
						PasswordValue.FromValue(SevenBitAsciiString.FromValue(password))).ToWireValue())));
	}

	public static PasswordParameter CreatePasswordParameter(
		PasswordLevelNumber passwordLevel,
		string password,
		CommunicationsAddress suppliedAddress)
	{
		return PasswordParameter.FromFields(
			PasswordLevel.FromValue(passwordLevel),
			Password.FromValue(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue(password))),
			suppliedAddress);
	}

	public static RouterCurrentParameterProjection CreateCurrentParameters(
		CommunicationsAddress localAddress,
		byte? brigade = null)
	{
		var level1PasswordVerifier = PasswordVerifier.Create(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
			PasswordVerifierWorkFactor.Default);

		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			brigade is null
				? localAddress.Brigade.Value
				: BrigadeOrAgencyIdentifier.FromValue(brigade.Value),
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(
					PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress),
			level1PasswordVerifier,
			level1PasswordVerifier,
			level1PasswordVerifier,
			level1PasswordVerifier,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)));
	}

	public static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}

internal class InMemoryPasswordVerifierStore : IRouterPasswordVerifierStore
{
	public Dictionary<(ParameterTable Table, ParameterNumber Number), PasswordVerifier> StoredValues { get; } = [];

	public ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken = default)
	{
		this.StoredValues.TryGetValue((parameterTable, parameterNumber), out var passwordVerifier);
		return ValueTask.FromResult(passwordVerifier);
	}

	public virtual ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		this.StoredValues[(parameterTable, parameterNumber)] = passwordVerifier;
		return ValueTask.CompletedTask;
	}
}

internal sealed class BlockingPasswordVerifierStore : InMemoryPasswordVerifierStore
{
	private readonly TaskCompletionSource storeCompleted = new(
		TaskCreationOptions.RunContinuationsAsynchronously);

	public TaskCompletionSource StoreStarted { get; } = new(
		TaskCreationOptions.RunContinuationsAsynchronously);

	public override async ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		this.StoreStarted.TrySetResult();
		await this.storeCompleted.Task.WaitAsync(cancellationToken);
		await base.StoreAsync(parameterTable, parameterNumber, passwordVerifier, cancellationToken);
	}

	public void CompleteStore()
	{
		this.storeCompleted.TrySetResult();
	}
}
