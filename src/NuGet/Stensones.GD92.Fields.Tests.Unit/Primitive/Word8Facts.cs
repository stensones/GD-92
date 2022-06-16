namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word8Facts
{
	[Fact]
	public void Value_Always_AsPassedInCtor()
	{
		var sut = new Word8(123);
		Assert.Equal<byte>(123, sut.Value);
	}
	
	[Fact]
	public void Value_Always_IsUnsigned8Bit()
	{
		var sut = new Word8(0);
		Assert.IsAssignableFrom<byte>(sut.Value);
	}

	[Fact]
	public void Value_WhenSet_IsReturned()
	{
		var sut = new Word8(12);
		sut.Value = 69;
		Assert.Equal<byte>(69, sut.Value);
	}
}