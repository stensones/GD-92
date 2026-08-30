using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

Console.WriteLine("Hello, World!");

var textMessage = Stensones.GD92.Messages.Text.FromFields(
	Block.FromValue(1),
	OfBlocks.FromValue(1),
	Stensones.GD92.Fields.Text.FromValue("My cock's a kipper"));

Console.WriteLine("object built text message");
Console.WriteLine(textMessage.ToString());
Console.WriteLine(string.Join(',', textMessage.ToWireValue().Select(_ => _.ToString("X2")).ToArray()));

var message = Envelope.FromValues(
	CommunicationsAddress.FromValues(
		Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)), 
		Node.FromValue(NodeIdentifier.FromValue(100)), 
		Port.FromValue(PortIdentifier.FromValue(0))),
	Destinations.FromAddresses(
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)), 
			Node.FromValue(NodeIdentifier.FromValue(101)), 
			Port.FromValue(PortIdentifier.FromValue(0))),
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)), 
			Node.FromValue(NodeIdentifier.FromValue(110)), 
			Port.FromValue(PortIdentifier.FromValue(0)))),
	ProtocolAndPriority.FromValues(
		MessagePriority.FromValue(MessagePriorityLevel.FromValue(8)), 
		ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(1))),
	AcknowledgementAndSequence.FromValues(
		SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(4567)),
		AcknowledgementRequest.NotRequested),
	textMessage);

Console.WriteLine("object build envelope");
Console.WriteLine(message.ToString());
Console.WriteLine(string.Join(',', message.ToWireValue().Select(_ => _.ToString("X2")).ToArray()));

var buffer = new EncodedMessageBuffer(message.ToWireValue());
var decodedMessage = Envelope.FromEncodedMessageBuffer(ref buffer);

Console.WriteLine("decoded envelope");
Console.WriteLine(decodedMessage.ToString());

Console.WriteLine("decoded mesage");
var decodedTextMessage = decodedMessage.Contents as Stensones.GD92.Messages.Text;
Console.WriteLine(decodedTextMessage.ToString());
