namespace Router.Domain.Messages;

public interface IMessage
{
	MessageTypes MessageType { get; init; }
}
