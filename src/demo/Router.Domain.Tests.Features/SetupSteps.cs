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

	[Given(@"the following current parameter table")]
	public void GivenTheFollowingConfiguration(Table table)
	{
		foreach(var row in table.Rows)
		{
			var name = row["name"];
			var value = row["value"];
			var parameter = ParameterFactory.Create(name, value);
		}
		//throw new PendingStepException();
	}

	[Given(@"the following GD-92 bits are running")]
	public void GivenTheFollowingGDBitsAreRunning(Table table)
	{
		//throw new PendingStepException();
	}
}
