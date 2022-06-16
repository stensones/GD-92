namespace Stensones.GD92.Fields.Tests.Unit.Primitive;

public class ShortStringFacts
{
	[Fact]
	public void Value_WhenDataIsAscii_AsPassedInCtor()
	{
		var sut = new ShortString("bob");
		Assert.Equal("bob", sut.Value);
	}

	[Fact]
	public void Value_WhenDataIsNotAscii_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new ShortString("stensønes"));
	}

	[Fact]
	public void Value_WhenDataIsNull_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new ShortString(null));
	}

	[Fact]
	public void Value_Always_IsUnsigned8Bit()
	{
		var sut = new ShortString("baldrik");
		Assert.IsAssignableFrom<string>(sut.Value);
	}

	[Fact]
	public void Value_WhenSet_IsReturned()
	{
		var sut = new ShortString("skdjfh");
		sut.Value = "Kate";
		Assert.Equal("Kate", sut.Value);
	}
}