using Stensones.GD92.Fields;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class RabbitMqUserAgentIngressListenerExtensions
{
	public static WolverineOptions ListenForUserAgentIngress(
		this WolverineOptions options,
		CommunicationsAddress localUserAgent)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(localUserAgent);

		options.ListenToRabbitQueue(UserAgentIngressEndpoint.QueueNameFrom(localUserAgent));
		options.Discovery.IncludeType<UserAgentIngressTransportMessageHandler>();

		return options;
	}
}
