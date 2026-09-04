using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

public sealed class RouterParameterRequestHandler
{
	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjection currentParameters;

	public RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion)
		: this(
			localAddress,
			protocolVersion,
			RouterCurrentParameterProjection.FromBrigadeOrAgencyIdentifier(
				BrigadeOrAgencyIdentifier.FromValue(localAddress.ToWireValue()[0])))
	{
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

	internal RouterEnvelopeHandlingResult Handle(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not ParameterRequest parameterRequest)
		{
			return RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.MessageTypeNotHandled);
		}

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
					this.currentParameters.BrigadeOrAgencyIdentifier.ToWireValue())));

		return RouterEnvelopeHandlingResult.Responded(response);
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
