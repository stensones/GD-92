using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Router.Persistence;

namespace Router;

public sealed class RouterParameterRequestHandler
{
	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjection? currentParameters;
	private readonly RouterCurrentParameterProjectionSource? currentParameterSource;
	private readonly IRouterLevel1PasswordVerifierStore? passwordVerifierStore;
	private static readonly PasswordLevel LevelZero =
		PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated);
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.Level1);
	public RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
	}

	public RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjection currentParameters)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameters = currentParameters ?? throw new ArgumentNullException(nameof(currentParameters));
	}

	public RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource ??
			throw new ArgumentNullException(nameof(currentParameterSource));
	}

	public RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource,
		IRouterLevel1PasswordVerifierStore passwordVerifierStore)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource ??
			throw new ArgumentNullException(nameof(currentParameterSource));
		this.passwordVerifierStore = passwordVerifierStore ??
			throw new ArgumentNullException(nameof(passwordVerifierStore));
	}

	internal RouterEnvelopeHandlingResult Handle(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is ParameterRequest parameterRequest)
		{
			return this.HandleParameterRequest(envelope, parameterRequest);
		}

		if (envelope.Contents is SetParameter setParameter)
		{
			return this.HandleSetParameter(envelope, setParameter);
		}

		return RouterEnvelopeHandlingResult.NotHandled(
			RouterEnvelopeHandlingStatus.MessageTypeNotHandled);
	}

	internal async ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is SetParameter setParameter &&
			setParameter.ParameterNumber == RouterParameterCatalogue.Level1PasswordNumber)
		{
			return await this.HandleLevel1PasswordChangeAsync(
				envelope,
				setParameter,
				cancellationToken);
		}

		return this.Handle(envelope);
	}

	private RouterEnvelopeHandlingResult HandleParameterRequest(
		Envelope envelope,
		ParameterRequest parameterRequest)
	{
		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != this.localAddress)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.DestinationNotHandled);
		}

		if (parameterRequest.ParameterTable != ParameterTable.Current ||
			parameterRequest.ParameterNumber != RouterParameterCatalogue.BrigadeOrAgency.Number)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		var response = Envelope.CreateParameterResponse(
			envelope,
			this.localAddress,
			this.protocolVersion,
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(
					(this.currentParameterSource?.GetCurrent()?.BrigadeOrAgencyIdentifier ??
						this.currentParameters?.BrigadeOrAgencyIdentifier ??
						this.localAddress.Brigade.Value)
						.ToWireValue())));

		return RouterEnvelopeHandlingResult.Responded(response);
	}

	private RouterEnvelopeHandlingResult HandleSetParameter(
		Envelope envelope,
		SetParameter setParameter)
	{
		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != this.localAddress)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.DestinationNotHandled);
		}

		if (setParameter.ParameterTable != ParameterTable.Current ||
			setParameter.ParameterNumber != RouterParameterCatalogue.CurrentPassword.Number)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		var valueBuffer = new EncodedMessageBuffer(setParameter.ParameterValue.ToWireValue());
		var submittedPassword = PasswordParameter.FromEncodedMessageBuffer(ref valueBuffer);

		if (this.currentParameterSource is null)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		if (submittedPassword.Level == LevelZero)
		{
			this.currentParameterSource.TryLogOffAtLevelZero(this.localAddress);

			return RouterEnvelopeHandlingResult.Responded(Envelope.CreateAcknowledgement(
				envelope,
				this.localAddress,
				this.protocolVersion));
		}

		if (submittedPassword.Level != LevelOne)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		if (!this.currentParameterSource.TryLogOnAtLevelOne(submittedPassword))
		{
			return RouterEnvelopeHandlingResult.Responded(Envelope.CreateNegativeAcknowledgement(
				envelope,
				this.localAddress,
				this.protocolVersion,
				envelope.Destinations,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidPassword)));
		}

		return RouterEnvelopeHandlingResult.Responded(Envelope.CreateAcknowledgement(
			envelope,
			this.localAddress,
			this.protocolVersion));
	}

	private async ValueTask<RouterEnvelopeHandlingResult> HandleLevel1PasswordChangeAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != this.localAddress)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.DestinationNotHandled);
		}

		if (setParameter.ParameterTable == ParameterTable.Permanent)
		{
			return this.CreateParameterNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		if (setParameter.ParameterTable != ParameterTable.Current &&
			setParameter.ParameterTable != ParameterTable.NonVolatile)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		if (this.currentParameterSource is null ||
			this.passwordVerifierStore is null ||
			!this.currentParameterSource.HasActiveNodeLoginAtLevelOne())
		{
			return this.CreateParameterNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		var valueBuffer = new EncodedMessageBuffer(setParameter.ParameterValue.ToWireValue());
		var password = Password.FromEncodedMessageBuffer(ref valueBuffer);
		if (valueBuffer.RemainingBitCount != 0)
		{
			return this.CreateParameterNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		var passwordVerifier = PasswordVerifier.Create(
			password.Value,
			PasswordVerifierWorkFactor.Default);
		if (setParameter.ParameterTable == ParameterTable.NonVolatile)
		{
			await this.passwordVerifierStore.StoreAsync(
				ParameterTable.NonVolatile,
				passwordVerifier,
				cancellationToken);
		}

		if (setParameter.ParameterTable == ParameterTable.Current &&
			!this.currentParameterSource.TryChangeLevel1Password(passwordVerifier))
		{
			return this.CreateParameterNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return RouterEnvelopeHandlingResult.Responded(Envelope.CreateAcknowledgement(
			envelope,
			this.localAddress,
			this.protocolVersion));
	}

	private RouterEnvelopeHandlingResult CreateParameterNegativeAcknowledgement(
		Envelope envelope,
		ParameterReasonCode reasonCode)
	{
		return RouterEnvelopeHandlingResult.Responded(Envelope.CreateNegativeAcknowledgement(
			envelope,
			this.localAddress,
			this.protocolVersion,
			envelope.Destinations,
			ReasonCode.FromParameterReasonCode(reasonCode)));
	}
}

internal enum RouterEnvelopeHandlingStatus
{
	Responded,
	MessageTypeNotHandled,
	DestinationNotHandled,
	ParameterNotHandled
}

internal sealed class RouterEnvelopeHandlingResult
{
	private RouterEnvelopeHandlingResult(
		RouterEnvelopeHandlingStatus status,
		Envelope? response)
	{
		this.Status = status;
		this.Response = response;
	}

	public RouterEnvelopeHandlingStatus Status { get; }
	public Envelope? Response { get; }

	public static RouterEnvelopeHandlingResult Responded(Envelope response)
	{
		ArgumentNullException.ThrowIfNull(response);

		return new RouterEnvelopeHandlingResult(RouterEnvelopeHandlingStatus.Responded, response);
	}

	public static RouterEnvelopeHandlingResult NotHandled(RouterEnvelopeHandlingStatus status)
	{
		if (status == RouterEnvelopeHandlingStatus.Responded)
		{
			throw new ArgumentOutOfRangeException(nameof(status));
		}

		return new RouterEnvelopeHandlingResult(status, null);
	}
}
