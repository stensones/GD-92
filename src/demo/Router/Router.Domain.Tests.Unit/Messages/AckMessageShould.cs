using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit.Messages;

public class AckMessageShould
{
	[Fact]
	public void HaveMessageType51()
	{
		var sut = new AckMessage();
		Assert.Equal(MessageTypes.Ack, sut.MessageType);
	}
}
