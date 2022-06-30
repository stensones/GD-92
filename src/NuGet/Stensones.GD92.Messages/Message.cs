using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stensones.GD92.Messages;
public abstract class Message
{
	public byte MessageNumber { get; private set; }

	public Message(byte messageNumber)
	{
		this.MessageNumber = messageNumber;
	}
}
