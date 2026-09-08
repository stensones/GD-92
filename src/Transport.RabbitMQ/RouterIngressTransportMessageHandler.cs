using Microsoft.Extensions.DependencyInjection;
using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RouterIngressTransportMessageHandler
{
	public async Task HandleAsync(
		RouterIngressTransportMessage message,
		IServiceScopeFactory serviceScopeFactory,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(serviceScopeFactory);
		cancellationToken.ThrowIfCancellationRequested();

		var envelope = IngressEnvelopeTransport.DecodeEnvelope(message.EnvelopeWireValue);

		using var scope = serviceScopeFactory.CreateScope();
		var receiver = scope.ServiceProvider.GetRequiredService<IRouterIngressReceiver>();
		await receiver.ReceiveAsync(envelope, cancellationToken);
	}
}
