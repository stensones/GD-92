using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain;

public class Envelope : IEnvelope
{
	public IMessage? Message { get; set; }
	public CommsAddress Source { get; set; }
	public IEnumerable<CommsAddress> Destinations { get; set; }
	public byte Priority { get; set; }
	public bool AckReq { get; set; }
	public ushort Seq { get; set; }
	public MessageTypes MessageType { get; set; }
	public uint Length { get; }
	public byte ProtVers { get; }
	public byte BCC { get; }
}
