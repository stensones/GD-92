namespace Stensones.GD92.Fields;

using System.IO;
using Stensones.GD92.Core;

// unsigned 16 bit value range 0-65535
public class Word16 : Field<ushort>, IValueObject
{
	public Word16(ushort initialValue) : base(initialValue)
	{
	}

	public override void WriteToStream(Stream stream)
	{
		var msb = (byte)(this.Value >> 8);
		var lsb = (byte)(this.Value & 0x00ff);
		stream.WriteByte(msb);
		stream.WriteByte(lsb);
	}
}