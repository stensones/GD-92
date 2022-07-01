using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain;

public class EnvelopeFactory
{
	public static IEnvelope Create(CommsAddress source, IEnumerable<CommsAddress> destinations, IMessage message)
	{
		throw new NotImplementedException();
	}
}
