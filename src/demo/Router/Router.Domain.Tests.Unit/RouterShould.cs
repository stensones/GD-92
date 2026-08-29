using Moq;
using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit;

public class RouterShould
{
	private readonly Mock<IEnvelope> envelope;

	public RouterShould()
	{
		this.envelope = new Mock<IEnvelope>();
	}

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
		var called = false;
		router.ForwardToLocalNode += (s, e) => called = true;
		this.envelope
			.Setup(_ => _.Destinations)
			.Returns(
				new List<CommsAddress> 
				{ 
					new CommsAddress("26:234:4")
				});
		
		router.Route(this.envelope.Object);

		Assert.True(called);
	}
}
