namespace Router.Domain.Messages;

public class AckMessage : IMessage
{
	public MessageTypes MessageType { get; init; }

	public AckMessage()
	{
		this.MessageType = MessageTypes.Ack;
	}
}
