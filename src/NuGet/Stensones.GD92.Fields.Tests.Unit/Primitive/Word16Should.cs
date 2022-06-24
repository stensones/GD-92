namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word16Should
{
	[Fact]
	public void Set_Value_As_Passed_In_Ctor()
	{
		var sut = new Word16(32123);
		Assert.Equal<ushort>(32123, sut.Value);
	}
	
	[Fact]
	public void Have_Value_That_Is_Unsigned_16Bit()
	{
		var sut = new Word16(0);
		Assert.IsAssignableFrom<ushort>(sut.Value);
	}
}