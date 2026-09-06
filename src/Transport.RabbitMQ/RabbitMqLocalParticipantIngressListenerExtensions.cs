using Stensones.GD92.Fields;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class RabbitMqLocalParticipantIngressListenerExtensions
{
	public static WolverineOptions ListenForLocalParticipantIngress(
		this WolverineOptions options,
		CommunicationsAddress localParticipant)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(localParticipant);

		options.ListenToRabbitQueue(LocalParticipantIngressEndpoint.QueueNameFrom(localParticipant));
		options.Discovery.IncludeType<LocalParticipantIngressTransportMessageHandler>();

		return options;
	}
}
