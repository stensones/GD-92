using Router.Domain.Messages;

namespace Router.Domain;

public interface IRouter
{
	event EventHandler<EventArgs<byte>> ForwardToLocalNode;

	void Route(IEnvelope envelope);
}
