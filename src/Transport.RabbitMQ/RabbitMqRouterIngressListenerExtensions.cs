using Stensones.GD92.Fields;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class RabbitMqRouterIngressListenerExtensions
{
	public static WolverineOptions ListenForRouterIngress(
		this WolverineOptions options,
		CommunicationsAddress localRouter)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(localRouter);

		options.ListenToRabbitQueue(RouterIngressEndpoint.QueueNameFrom(localRouter));
		options.Discovery.IncludeType<RouterIngressTransportMessageHandler>();

		return options;
	}
}
