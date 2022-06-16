namespace Stensones.GD92.Fields;

using Stensones.GD92.Core;

// unsigned 32 bit value range 0-4billion
public class Word32 : Field<uint>, IValueObject
{
	public Word32(uint initialValue) : base(initialValue)
	{
	}
}