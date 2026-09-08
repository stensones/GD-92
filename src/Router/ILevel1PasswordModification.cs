using Stensones.GD92.Messages;

namespace Router;

internal interface ILevel1PasswordModification
{
	ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken);
}
