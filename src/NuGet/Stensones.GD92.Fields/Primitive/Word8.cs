namespace Stensones.GD92.Fields;

using Stensones.GD92.Core;

// unsigned 8 bit value range 0-255
public class Word8 : Field<byte>, IValueObject
{
	public Word8(byte initialValue) : base(initialValue)
	{
	}
}