using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class ProtocolAndPrioritySteps
{
	private MessagePriority? priority;
	private ProtocolVersion? protocolVersion;
	private ProtocolAndPriority? protocolAndPriority;

	[Given(@"valid Message Priority (.*) and Protocol Version (.*) values")]
	public void GivenValidMessagePriorityAndProtocolVersionValues(byte priority, byte protocolVersion)
	{
		this.priority = MessagePriority.FromValue(MessagePriorityLevel.FromValue(priority));
		this.protocolVersion = ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion));
	}

	[When(@"a ProtocolAndPriority field is created")]
	public void WhenAProtocolAndPriorityFieldIsCreated()
	{
		this.protocolAndPriority = ProtocolAndPriority.FromValues(this.priority!, this.protocolVersion!);
	}

	[Then(@"its protocol and priority field bytes are ""(.*)""")]
	public void ThenItsProtocolAndPriorityFieldBytesAre(string expectedBytes)
	{
		this.protocolAndPriority!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
