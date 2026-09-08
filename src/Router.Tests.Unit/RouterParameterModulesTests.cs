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
		IRouterParameterRead parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var result = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Current),
			CancellationToken.None);

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.Responded);
		var response = result.Response!;
		response.Contents.Should().BeOfType<Parameter>().Which.ParameterValue.ToWireValue()
			.Should().Equal([42]);
	}

	[Fact]
	public async Task Leaves_an_unsupported_Router_Parameter_unhandled()
	{
		var localAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		IRouterParameterRead parameterRead = new RouterParameterRead(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion);

		var result = await parameterRead.HandleAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				localAddress,
				ParameterTable.Permanent),
			CancellationToken.None);

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.ParameterNotHandled);
		result.Response.Should().BeNull();
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
		INodeLogin nodeLogin = new NodeLogin(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var result = await nodeLogin.HandleAsync(
			RouterParameterModuleTestSupport.CreateCurrentPasswordSet(
				localAddress,
				PasswordLevelNumber.Level1,
				"WATER",
				RouterParameterModuleTestSupport.CreateAddress(26, 100, 25)),
			CancellationToken.None);

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.Responded);
		result.Response!.Contents.Should().BeOfType<NegativeAcknowledgement>()
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
		INodeLogin nodeLogin = new NodeLogin(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source);

		var result = await nodeLogin.HandleAsync(
			RouterParameterModuleTestSupport.CreateCurrentPasswordSet(
				localAddress,
				PasswordLevelNumber.Unauthenticated,
				"ignored",
				RouterParameterModuleTestSupport.CreateAddress(42, 200, 7)),
			CancellationToken.None);

		result.Response!.Contents.Should().BeOfType<Acknowledgement>();
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
		ILevel1PasswordModification modification = new Level1PasswordModification(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source,
			store);

		var result = await modification.HandleAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				localAddress,
				ParameterTable.Current,
				"WATER"),
			CancellationToken.None);

		result.Response!.Contents.Should().BeOfType<Acknowledgement>();
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
		ILevel1PasswordModification modification = new Level1PasswordModification(
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
		var result = await handling;

		result.Response!.Contents.Should().BeOfType<Acknowledgement>();
		store.StoredValues.Should().ContainKey(ParameterTable.NonVolatile);
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
		ILevel1PasswordModification modification = new Level1PasswordModification(
			localAddress,
			RouterParameterModuleTestSupport.ProtocolVersion,
			source,
			new InMemoryPasswordVerifierStore());

		var result = await modification.HandleAsync(
			RouterParameterModuleTestSupport.CreateLevel1PasswordChange(
				localAddress,
				ParameterTable.Permanent,
				"WATER"),
			CancellationToken.None);

		result.Response!.Contents.Should().BeOfType<NegativeAcknowledgement>()
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
				RouterParameterCatalogue.BrigadeOrAgency.Number));
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
		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			brigade is null
				? localAddress.Brigade.Value
				: BrigadeOrAgencyIdentifier.FromValue(brigade.Value),
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(
					PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress),
			PasswordVerifier.Create(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				PasswordVerifierWorkFactor.Default),
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

internal class InMemoryPasswordVerifierStore : IRouterLevel1PasswordVerifierStore
{
	public Dictionary<ParameterTable, PasswordVerifier> StoredValues { get; } = [];

	public ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		CancellationToken cancellationToken = default)
	{
		this.StoredValues.TryGetValue(parameterTable, out var passwordVerifier);
		return ValueTask.FromResult(passwordVerifier);
	}

	public virtual ValueTask StoreAsync(
		ParameterTable parameterTable,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		this.StoredValues[parameterTable] = passwordVerifier;
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
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		this.StoreStarted.TrySetResult();
		await this.storeCompleted.Task.WaitAsync(cancellationToken);
		await base.StoreAsync(parameterTable, passwordVerifier, cancellationToken);
	}

	public void CompleteStore()
	{
		this.storeCompleted.TrySetResult();
	}
}
