using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router.Domain.Fields;

public record CommsAddress
{
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
			this.Port = byte.Parse(parts[2]);
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
