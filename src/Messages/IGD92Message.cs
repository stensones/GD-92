using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public interface IGD92Message
{
	MessageType Type { get; }
}
