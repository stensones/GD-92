namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class Word16Facts
{
	[Fact]
	public void Value_Always_AsPassedInCtor()
	{
		var sut = new Word16(32123);
		Assert.Equal<ushort>(32123, sut.Value);
	}
	
	[Fact]
	public void Value_Always_IsUnsigned8Bit()
	{
		var sut = new Word16(0);
		Assert.IsAssignableFrom<ushort>(sut.Value);
	}

	[Fact]
	public void Value_WhenSet_IsReturned()
	{
		var sut = new Word16(12);
		sut.Value = 69;
		Assert.Equal<ushort>(69, sut.Value);
	}
}