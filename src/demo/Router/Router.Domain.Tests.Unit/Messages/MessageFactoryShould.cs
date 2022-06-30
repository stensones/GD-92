using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit.Messages;

public class MessageFactoryShould
{
	[Fact]
	public void ThrowWhenMessageTypeIsOutOfRange()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => MessageFactory.Create((MessageTypes)3000));
	}

	[Fact]
	public void ThrowWhenMessageTypeIsInvalid()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => MessageFactory.Create(MessageTypes.Invalid));
	}

	[Fact]
	public void CreateAckMessage()
	{
		var ackMessage = MessageFactory.Create(MessageTypes.Ack);
		Assert.NotNull(ackMessage);
		Assert.Equal(MessageTypes.Ack, ackMessage.MessageType);
	}
}