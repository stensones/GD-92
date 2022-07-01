using Microsoft.Extensions.DependencyInjection;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Features;

public class RouterContext
{
	private ServiceProvider? provider;

	public IServiceCollection ServiceCollection { get; init; }

	public IRouter? Router => this.provider?.GetService<IRouter>();

	public IMessage? ReceivedMessage { get; internal set; }

	public IEnvelope? Envelope { get; internal set; }

	public RouterContext()
	{
		this.ServiceCollection = new ServiceCollection();
	}

	public void LoadRouterDomain()
	{
		this.ServiceCollection.AddRouter();
		this.provider = this.ServiceCollection.BuildServiceProvider();
	}
}