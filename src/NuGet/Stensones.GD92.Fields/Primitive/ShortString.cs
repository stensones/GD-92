namespace Stensones.GD92.Fields;

using Stensones.GD92.Core;

// short string 0-255 chars long, ASCII characters, no compression
public class ShortString : Field<string>, IValueObject
{
	public ShortString(string initalValue)
		: base(initalValue)
	{
		if (string.IsNullOrEmpty(initalValue)) 
		{
			throw new ArgumentOutOfRangeException(nameof(initalValue), "value of string field cannot be null or empty");
		}

		if (initalValue.Length > 255)
		{
			throw new ArgumentOutOfRangeException(nameof(initalValue), "value of string field cannot exceed 255 character length");
		}

		if (this.HasNonASCIIChars(initalValue))
		{
			throw new ArgumentOutOfRangeException(nameof(initalValue), "value of string field cannot contain non ACSII characters");
		}
	}

	private bool HasNonASCIIChars(string str)
	{
		return (System.Text.Encoding.UTF8.GetByteCount(str) != str.Length);
	}
}