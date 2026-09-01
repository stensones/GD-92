using System.Globalization;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Transport.RabbitMQ;

public static class RouterIngressEndpoint
{
	public static string QueueNameFrom(CommunicationsAddress routerAddress)
	{
		ArgumentNullException.ThrowIfNull(routerAddress);

		return $"gd92.router.{routerAddress.Brigade.Value}.{routerAddress.Node.Value.ToString(CultureInfo.InvariantCulture)}";
	}

	public static Uri From(CommunicationsAddress routerAddress)
	{
		return new Uri(
			$"rabbitmq://queue/{QueueNameFrom(routerAddress)}");
	}
}
