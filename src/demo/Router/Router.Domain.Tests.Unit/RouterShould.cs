using Moq;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit;

public class RouterShould
{
	[Fact]
	public void Implement_IRouter_Interface()
	{
		var router = new Router();
		Assert.IsAssignableFrom<IRouter>(router);
	}

	[Fact]
	public void Route_WhenEnvelopeIsToOwnAddress_ForwardToPort()
	{
		var router = new Router();
		var envelope = new Mock<IEnvelope>();
		router.Route(envelope.Object);
	}
}
