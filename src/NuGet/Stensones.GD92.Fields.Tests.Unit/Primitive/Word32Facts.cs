namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word32Facts
{
	[Fact]
	public void Value_Always_AsPassedInCtor()
	{
		var sut = new Word32(20032123);
		Assert.Equal<uint>(20032123, sut.Value);
	}
	
	[Fact]
	public void Value_Always_IsUnsigned8Bit()
	{
		var sut = new Word32(0);
		Assert.IsAssignableFrom<uint>(sut.Value);
	}

	[Fact]
	public void Value_WhenSet_IsReturned()
	{
		var sut = new Word32(12);
		sut.Value = 69;
		Assert.Equal<uint>(69, sut.Value);
	}
}