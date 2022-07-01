using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router.Domain.Fields;

public record CommsAddress
{
	//	<comms_address> 3 bytes(24 bits), binary encoded as follows:
	//bits 0-7 brigade_identifier(0-255)
	//bits 8-17 node_identifier(0-1023)
	//bits 18-23 port_identifier(0-63)
	public byte Brigade { get; init; }
	public ushort Node { get; init; }
	public byte Port { get; init; }

	public CommsAddress(byte brigade, ushort node, byte port)
	{
		this.Brigade = brigade;
		this.Node = node;
		this.Port = port;
	}

	public CommsAddress(string sourceStr)
	{
		if (sourceStr.Length < 5 || sourceStr.Count(_=>_ == ':') != 2)
		{
			throw new ArgumentOutOfRangeException(nameof(sourceStr));
		}

		try
		{
			var parts = sourceStr.Split(':');
			
			this.Brigade = byte.Parse(parts[0]);
			
			this.Node = ushort.Parse(parts[1]);
			if (this.Node > 1023)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceStr));
			}

			this.Port = byte.Parse(parts[2]);
			if (this.Port > 63)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceStr));
			}
		}
		catch (OverflowException)
		{
			throw new ArgumentOutOfRangeException(nameof(sourceStr));
		}
		catch (FormatException)
		{
			throw new ArgumentOutOfRangeException(nameof(sourceStr));
		}
	}
}
