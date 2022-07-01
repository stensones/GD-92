using Moq;
using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit;

public class EnvelopeFactoryShould
{
	[Fact]
	public void Create_WithValidParameters_ReturnsNewEnvelope()
	{
		var source = new CommsAddress(26, 1, 10);
		var destinations = new List<CommsAddress> 
		{ 
			new CommsAddress(26, 1, 10)
		};
		var message = new Mock<IMessage>();

		var envelope = EnvelopeFactory.Create(source, destinations, message.Object);
		
		Assert.NotNull(envelope);
	}
}
