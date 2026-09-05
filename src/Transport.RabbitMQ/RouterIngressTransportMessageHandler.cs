using Microsoft.Extensions.DependencyInjection;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

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

		var buffer = new EncodedMessageBuffer(message.EnvelopeWireValue);
		var envelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("The encoded Router ingress Envelope contains trailing bytes.");
		}

		using var scope = serviceScopeFactory.CreateScope();
		var receiver = scope.ServiceProvider.GetRequiredService<IRouterIngressReceiver>();
		await receiver.ReceiveAsync(envelope, cancellationToken);
	}
}
