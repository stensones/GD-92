using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterTableTests
{
	[Fact]
	public void Serializes_the_current_parameter_table()
	{
		ParameterTable.FromValue(ParameterTableIdentifier.Current)
			.ToWireValue().Should().Equal(new byte[] { 0x02 });
	}

	[Fact]
	public void Rejects_parameter_table_identifiers_outside_the_defined_encodings()
	{
		Action createParameterTableIdentifier = () => ParameterTableIdentifier.FromValue(3);

		createParameterTableIdentifier.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Does_not_expose_a_primitive_factory()
	{
		typeof(ParameterTable).GetMethod("FromValue", [typeof(byte)]).Should().BeNull();
	}
}
