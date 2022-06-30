namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class ShortCompressesStringShould
{
	[Fact]
	public void Set_Value_When_Constructor_Data_Is_Ascii()
	{
		var sut = new ShortString("bob");
		Assert.Equal("bob", sut.Value);
	}

	[Fact]
	public void Inherit_From_Field_Of_String()
	{
		var sut = new ShortString("I am a teapot!");
		Assert.IsAssignableFrom<Field<string>>(sut);
	}

	[Fact]
	public void Throw_When_Constructor_Data_Over_255_Chars_Long()
	{
		var inputValue = "".PadRight(300, 'x');
		Assert.Throws<ArgumentOutOfRangeException>(() => new ShortString(inputValue));
	}

	[Fact]
	public void Throw_When_Constructor_Data_Is_Not_Ascii()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new ShortString("stensønes"));
	}

	[Fact]
	public void Throw_When_Constructor_Data_Is_Null()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new ShortString(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact]
	public void Have_Value_That_Is_String()
	{
		var sut = new ShortString("baldrik");
		Assert.IsAssignableFrom<string>(sut.Value);
	}
}