using System.Text;
using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class ProtocolFieldWireValueSteps
{
	private ProtocolField? protocolField;
	private byte[]? serializedWireValue;

	[Given(@"a Protocol Field with wire value ""(.*)""")]
	public void GivenAProtocolFieldWithWireValue(string wireValue)
	{
		this.protocolField = ProtocolField.FromWireValue(Encoding.ASCII.GetBytes(wireValue));
	}

	[When(@"the Protocol Field is serialized")]
	public void WhenTheProtocolFieldIsSerialized()
	{
		this.serializedWireValue = this.protocolField!.ToWireValue();
	}

	[Then(@"the serialized wire value is ""(.*)""")]
	public void ThenTheSerializedWireValueIs(string wireValue)
	{
		this.serializedWireValue.Should().Equal(Encoding.ASCII.GetBytes(wireValue));
	}
}
