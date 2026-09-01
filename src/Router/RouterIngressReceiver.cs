using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Router;

internal sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	private readonly RouterParameterRequestHandler parameterRequestHandler;

	public RouterIngressReceiver(RouterParameterRequestHandler parameterRequestHandler)
	{
		this.parameterRequestHandler = parameterRequestHandler ??
			throw new ArgumentNullException(nameof(parameterRequestHandler));
	}

	internal Envelope? Response { get; private set; }

	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		this.Response = this.parameterRequestHandler.Handle(envelope).Response;

		return Task.CompletedTask;
	}
}
