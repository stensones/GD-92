namespace Stensones.GD92.Fields;

using Stensones.GD92.Core;

// unsigned 16 bit value range 0-65535
public class Word16 : Field<ushort>, IValueObject
{
	public Word16(ushort initialValue) : base(initialValue)
	{
	}
}