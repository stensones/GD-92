using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Router;

internal sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	private readonly RouterParameterRequestHandler parameterRequestHandler;
	private readonly IUserAgentIngress userAgentIngress;

	public RouterIngressReceiver(
		RouterParameterRequestHandler parameterRequestHandler,
		IUserAgentIngress userAgentIngress)
	{
		this.parameterRequestHandler = parameterRequestHandler ??
			throw new ArgumentNullException(nameof(parameterRequestHandler));
		this.userAgentIngress = userAgentIngress ??
			throw new ArgumentNullException(nameof(userAgentIngress));
	}

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		var response = this.parameterRequestHandler.Handle(envelope).Response;

		if (response is not null)
		{
			await this.userAgentIngress.DeliverAsync(response, cancellationToken);
		}
	}
}
