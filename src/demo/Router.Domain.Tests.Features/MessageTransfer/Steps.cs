using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Features.MessageTransfer;

[Binding]
public class Steps
{
	private readonly RouterContext routerContext;

	public Steps(RouterContext routerContext)
	{
		this.routerContext = routerContext;
	}

	[Given(@"the router receives the following message")]
	public void GivenTheRouterReceivedTheFollowingMessage(Table table)
	{
		var messageNumber = GetMessageNumberFromTable(table);
		var source = GetSourceAddressFromTable(table);
		var destinations = GetDestAddressesFromTable(table);
		var message = MessageFactory.Create(messageNumber);
		var envelope = EnvelopeFactory.Create(source, destinations, message);
		//this.routerContext.Envelope = envelope;
		this.routerContext.ReceivedMessage = message;
	}

	[When(@"the router routes the message")]
	public void WhenTheRouterRoutesTheMessage()
	{
		var router = this.routerContext.Router;
		var envelope = this.routerContext.Envelope;
		router!.Route(envelope!);
		throw new PendingStepException();
	}

	[Then(@"the message is forwarded to port (.*)")]
	public void ThenTheMessageIsForwardedToPort(int p0)
	{
		throw new PendingStepException();
	}

	private static IEnumerable<CommsAddress> GetDestAddressesFromTable(Table table)
	{
		var destStr = table.Rows[0]["dest"];
		var destList = destStr.Split(',').Select(_ => new CommsAddress(_));
		return destList;
	}

	private static CommsAddress GetSourceAddressFromTable(Table table)
	{
		var sourceStr = table.Rows[0]["source"];
		return new CommsAddress(sourceStr);
	}

	private static MessageTypes GetMessageNumberFromTable(Table table)
	{
		return (MessageTypes)int.Parse(table.Rows[0]["number"]);
	}
}
