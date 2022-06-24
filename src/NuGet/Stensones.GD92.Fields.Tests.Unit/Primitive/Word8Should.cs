namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word8Should
{
	[Fact]
	public void Set_Value_As_Passed_In_Ctor()
	{
		var sut = new Word8(123);
		Assert.Equal<byte>(123, sut.Value);
	}
	
	[Fact]
	public void Have_Value_That_Is_Unsigned_8Bit()
	{
		var sut = new Word8(0);
		Assert.IsAssignableFrom<byte>(sut.Value);
	}
}