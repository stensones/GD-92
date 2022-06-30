using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Xml;
using System;
using Router.Domain.Messages;
using Router.Domain.Fields;

namespace Router.Domain;

public interface IEnvelope
{
	//	<source>, which defines the unique address of the source of the message;
	CommsAddress Source { get; set; }

	//- <dest_count>, which specifies the number of destination addresses which are
	//included in the envelope(minimum 1, maximum 63);
	//- <destination>, which defines<dest_count> unique addresses;
	IEnumerable<CommsAddress> Destinations { get; set; }

	//- <priority>, which defines the priority of the message(minimum 1, maximum 9)
	//with priority 1 being the highest priority;
	byte Priority { get; set; }

	//- <ack_req>, which indicates whether the UA sending the message expects an
	//acknowledgement(which may come from the destination UA or from the MTS);
	bool AckReq { get; set; }

	//- <seq>, which is a sequence number used to identify messages(see paragraph
	//3.3.4);
	ushort Seq { get; set; }

	//- <message_type>, which defines the type of the content of the message;
	MessageTypes MessageType { get; set; }

	//- <length>, which is the number of bytes in the<message> field;
	uint Length { get; }

	//- <message>, which is the message or contents of the envelope;
	IMessage? Message { get; set; }

	//- <prot_vers>, which defines the protocol version being used by the source UA.
	//Version 1 , except Page Officer message which is version 2.
	byte ProtVers { get; }

	//- <bcc>, which is a block check character which is used to confirm the end-to-end
	//integrity of the message.It is calculated by exclusive ORing of all of the preceding
	//bytes within the envelope
	byte BCC { get; }
}
