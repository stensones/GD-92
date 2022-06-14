namespace Stensones.GD92.Fields;

using Stensones.GD92.Core;

// unsigned 8 bit value range 0-255
public class Word8 : Field, IValueObject
{
    public byte Value { get; set; }  
}