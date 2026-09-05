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
			parameterRequest.ParameterNumber.Value != 1)
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
			setParameter.ParameterNumber.Value != 4)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		var valueBuffer = new EncodedMessageBuffer(setParameter.ParameterValue.ToWireValue());
		var submittedPassword = PasswordParameter.FromEncodedMessageBuffer(ref valueBuffer);

		if (this.currentParameterSource is null ||
			!this.currentParameterSource.TryLogOnAtLevelOne(submittedPassword))
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled);
		}

		return RouterEnvelopeHandlingResult.Responded(Envelope.CreateAcknowledgement(
			envelope,
			this.localAddress,
			this.protocolVersion));
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
