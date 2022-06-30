using Router.Domain.Messages;

namespace Router.Domain;

public interface IRouter
{
	void Route(IMessage message);
}
