using System.Globalization;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class UserAgentIngressEndpoint
{
	public static string QueueNameFrom(CommunicationsAddress userAgentAddress)
	{
		ArgumentNullException.ThrowIfNull(userAgentAddress);

		return $"gd92.ua.{userAgentAddress.Brigade.Value}.{userAgentAddress.Node.Value.ToString(CultureInfo.InvariantCulture)}.{userAgentAddress.Port.Value.ToString(CultureInfo.InvariantCulture)}";
	}

	public static Uri From(CommunicationsAddress userAgentAddress)
	{
		return new Uri($"rabbitmq://queue/{QueueNameFrom(userAgentAddress)}");
	}
}
