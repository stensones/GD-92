using AwesomeAssertions;
using NetArchTest.Rules;

namespace Stensones.GD92.Fields.Tests.Architecture;

public sealed class GD9FieldTests
{
	[Fact]
	public void Protocol_fields_are_public()
	{
		var result = Types.InAssembly(typeof(IGD9Field).Assembly)
			.That()
			.ImplementInterface(typeof(IGD9Field))
			.Should()
			.BePublic()
			.GetResult();

		result.IsSuccessful.Should().BeTrue();
	}
}
