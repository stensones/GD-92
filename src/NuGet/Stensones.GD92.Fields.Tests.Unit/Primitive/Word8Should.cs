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
	public void Inherit_From_Field_Of_Byte()
	{
		var sut = new Word8(123);
		Assert.IsAssignableFrom<Field<byte>>(sut);
	}

	[Fact]
	public void Have_Value_That_Is_Unsigned_8Bit()
	{
		var sut = new Word8(0);
		Assert.IsAssignableFrom<byte>(sut.Value);
	}

	[Fact]
	public void Write_Byte_To_Stream_When_Serialised()
	{
		const byte inptValue = 9;
		var sut = new Word8(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			Assert.Equal(1, strm.Length);
			Assert.Equal(1, strm.Position);
		}
	}

	[Fact]
	public void Have_Field_Value_In_First_Stream_Byte_When_Serialised()
	{
		const byte inptValue = 9;
		var sut = new Word8(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			strm.Flush();
			strm.Position = 0;
			Assert.Equal(inptValue, strm.ReadByte());
		}
	}
}