using Microsoft.Extensions.DependencyInjection;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Features;

public class RouterContext
{
	public IServiceCollection ServiceCollection { get; init; }

	public IRouter Router => this.Get<IRouter>();

	public IMessage? ReceivedMessage { get; internal set; }

	public IEnvelope? Envelope { get; internal set; }

	public RouterContext()
	{
		this.ServiceCollection = new ServiceCollection();
	}

	public void LoadRouterDomain()
	{
		this.ServiceCollection.AddRouter();
	}

	private T Get<T>()
	{
		var instance = this.TryGet<T>();
		if (instance == null)
		{
			throw new NullReferenceException();
		}

		return instance;
	}

	private T? TryGet<T>()
	{
		return (T?)this.ServiceCollection
			.SingleOrDefault(_=>_.ServiceType is (T))
			?.ImplementationInstance;
	}
}