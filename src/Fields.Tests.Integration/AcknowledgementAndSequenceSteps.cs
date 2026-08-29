using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class AcknowledgementAndSequenceSteps
{
	private SequenceNumber? sequenceNumber;
	private AcknowledgementRequest? acknowledgementRequest;
	private AcknowledgementAndSequence? acknowledgementAndSequence;

	[Given(@"Sequence Number (.*) and an Acknowledgement Request")]
	public void GivenSequenceNumberAndAnAcknowledgementRequest(ushort sequenceNumber)
	{
		this.sequenceNumber = SequenceNumber.FromValue(sequenceNumber);
		this.acknowledgementRequest = AcknowledgementRequest.Requested;
	}

	[When(@"an AcknowledgementAndSequence field is created")]
	public void WhenAnAcknowledgementAndSequenceFieldIsCreated()
	{
		this.acknowledgementAndSequence = AcknowledgementAndSequence.FromValues(
			this.sequenceNumber!,
			this.acknowledgementRequest!);
	}

	[Then(@"its acknowledgement and sequence field bytes are ""(.*)""")]
	public void ThenItsAcknowledgementAndSequenceFieldBytesAre(string expectedBytes)
	{
		this.acknowledgementAndSequence!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
