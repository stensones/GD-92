using Stensones.GD92.Messages;

namespace Router;

internal interface IRouterParameterRead
{
	ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken);
}
