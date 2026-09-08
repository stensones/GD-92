using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

public sealed class RouterParameterRequestHandler
{
	private readonly CommunicationsAddress localAddress;
	private readonly IRouterParameterRead routerParameterRead;
	private readonly INodeLogin nodeLogin;
	private readonly ILevel1PasswordModification level1PasswordModification;

	internal CommunicationsAddress LocalAddress => this.localAddress;

	internal RouterParameterRequestHandler(
		CommunicationsAddress localAddress,
		IRouterParameterRead routerParameterRead,
		INodeLogin nodeLogin,
		ILevel1PasswordModification level1PasswordModification)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.routerParameterRead = routerParameterRead ??
			throw new ArgumentNullException(nameof(routerParameterRead));
		this.nodeLogin = nodeLogin ?? throw new ArgumentNullException(nameof(nodeLogin));
		this.level1PasswordModification = level1PasswordModification ??
			throw new ArgumentNullException(nameof(level1PasswordModification));
	}

	internal ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not ParameterRequest and not SetParameter)
		{
			return ValueTask.FromResult(RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.MessageTypeNotHandled));
		}

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != this.localAddress)
		{
			return ValueTask.FromResult(RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.DestinationNotHandled));
		}

		return envelope.Contents switch
		{
			ParameterRequest => this.routerParameterRead.HandleAsync(envelope, cancellationToken),
			SetParameter
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} when table == ParameterTable.Current &&
				number == Router.Persistence.RouterParameterCatalogue.CurrentPassword.Number =>
				this.nodeLogin.HandleAsync(envelope, cancellationToken),
			SetParameter
			{
				ParameterNumber: var number
			} when number == Router.Persistence.RouterParameterCatalogue.Level1PasswordNumber =>
				this.level1PasswordModification.HandleAsync(envelope, cancellationToken),
			_ => ValueTask.FromResult(RouterEnvelopeHandlingResult.NotHandled(
				RouterEnvelopeHandlingStatus.ParameterNotHandled))
		};
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
