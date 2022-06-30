namespace Router.Domain.Tests.Unit;

public class EnvelopeShould
{
	[Fact]
	public void Implement_IEnvelope_Interface()
	{
		var envelope = new Envelope();
		Assert.IsAssignableFrom<IEnvelope>(envelope);
	}
}
