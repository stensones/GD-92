namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word32Should
{
	[Fact]
	public void Value_Always_AsPassedInCtor()
	{
		var sut = new Word32(20032123);
		Assert.Equal<uint>(20032123, sut.Value);
	}
	
	[Fact]
	public void Have_Value_That_Is_Unsigned_32Bit()
	{
		var sut = new Word32(0);
		Assert.IsAssignableFrom<uint>(sut.Value);
	}
}