using Router.Domain.Messages;

namespace Router.Domain;

public class Router : IRouter
{
	public event EventHandler<EventArgs<byte>>? ForwardToLocalNode;

	public void Route(IEnvelope envelope)
	{
		var port = envelope.Destinations?.FirstOrDefault()?.Port;
		if (port.HasValue)
		{
			this.OnForwardToLocalNode(port.Value);
		}
	}

	protected virtual void OnForwardToLocalNode(byte port)
	{
		this.ForwardToLocalNode?.Invoke(this, new EventArgs<byte>(port));
	}
}
