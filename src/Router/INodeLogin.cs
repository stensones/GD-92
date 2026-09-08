using Stensones.GD92.Messages;

namespace Router;

internal interface INodeLogin
{
	ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken);
}
