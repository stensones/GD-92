using System.Globalization;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class LocalParticipantIngressEndpoint
{
	public static string QueueNameFrom(CommunicationsAddress participantAddress)
	{
		ArgumentNullException.ThrowIfNull(participantAddress);

		return $"gd92.participant.{participantAddress.Brigade.Value}.{participantAddress.Node.Value.ToString(CultureInfo.InvariantCulture)}.{participantAddress.Port.Value.ToString(CultureInfo.InvariantCulture)}";
	}

	public static Uri From(CommunicationsAddress participantAddress)
	{
		return new Uri($"rabbitmq://queue/{QueueNameFrom(participantAddress)}");
	}
}
