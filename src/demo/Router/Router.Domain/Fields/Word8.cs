using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router.Domain.Fields;

public class Word8 : IField
{
	public byte Value { get; init; }

	public Word8(byte value)
	{
		this.Value = value;
	}
}
