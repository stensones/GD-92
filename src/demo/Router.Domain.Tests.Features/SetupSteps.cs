namespace Router.Domain.Tests.Features;

[Binding]
public class SetupSteps
{
	private readonly RouterContext routerContext;

	public SetupSteps(RouterContext routerContext)
	{
		this.routerContext = routerContext;
	}

	[Given(@"the router domain is loaded")]
	public void GivenTheRouterDomainIsLoaded()
	{
		this.routerContext.LoadRouterDomain();
	}

	[Given(@"the following configuration")]
	public void GivenTheFollowingConfiguration(Table table)
	{
		//throw new PendingStepException();
	}

	[Given(@"the following GD-92 bits are running")]
	public void GivenTheFollowingGDBitsAreRunning(Table table)
	{
		//throw new PendingStepException();
	}
}
