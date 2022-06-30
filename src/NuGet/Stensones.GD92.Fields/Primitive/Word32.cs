namespace Stensones.GD92.Fields;

using System.IO;
using Stensones.GD92.Core;

// unsigned 32 bit value range 0-4billion
public class Word32 : Field<uint>, IValueObject
{
	public Word32(uint initialValue) : base(initialValue)
	{
	}

	public override void WriteToStream(Stream stream)
	{
		var bytes = new byte[4];
		bytes[3] = (byte)((this.Value & 0xff000000) >> 24);
		bytes[2] = (byte)((this.Value & 0x00ff0000) >> 16);
		bytes[1] = (byte)((this.Value & 0x0000ff00) >> 8);
		bytes[0] = (byte)((this.Value & 0x000000ff) >> 0);
		foreach(var word8 in bytes)
		{
			stream.WriteByte(word8);
		}
		//var bytes = new byte[4];
		//var value = this.Value;
		//uint mask = 0xff000000;
		//for (var i = 3; i > -1; i--)
		//{
		//	mask = (mask >> (i * 8));
		//	bytes[i] = (byte)(value & mask);
		//	stream.WriteByte(bytes[i]);
		//}
	}
}