using AwesomeAssertions;
using NetArchTest.Rules;
using System.Reflection;

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

	[Fact]
	public void Protocol_fields_have_a_typed_encoded_message_buffer_factory()
	{
		var fieldTypes = typeof(IGD9Field).Assembly
			.GetTypes()
			.Where(type => type.IsClass && typeof(IGD9Field).IsAssignableFrom(type));

		foreach (var fieldType in fieldTypes)
		{
			var factory = fieldType.GetMethod(
				"FromEncodedMessageBuffer",
				BindingFlags.Public | BindingFlags.Static,
				[typeof(EncodedMessageBuffer).MakeByRefType()]);

			factory.Should().NotBeNull();
			factory!.ReturnType.Should().Be(fieldType);
		}
	}
}
