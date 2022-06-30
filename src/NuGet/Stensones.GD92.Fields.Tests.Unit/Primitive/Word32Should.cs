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
	public void Inherit_From_Field_Of_Uint()
	{
		var sut = new Word32(123);
		Assert.IsAssignableFrom<Field<uint>>(sut);
	}

	[Fact]
	public void Have_Value_That_Is_Unsigned_32Bit()
	{
		var sut = new Word32(0);
		Assert.IsAssignableFrom<uint>(sut.Value);
	}

	[Fact]
	public void Write_4_Bytes_To_Stream_When_Serialised()
	{
		const byte inptValue = 9;
		var sut = new Word32(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			Assert.Equal(4, strm.Length);
			Assert.Equal(4, strm.Position);
		}
	}

	[Fact]
	public void Have_Field_Value_In_First_Stream_Byte_When_Serialised()
	{
		const byte inptValue = 9;
		const byte b1 = 0x00;
		const byte b2 = 0x00;
		const byte b3 = 0x00;
		const byte b4 = 0x09;
		var sut = new Word32(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			strm.Flush();
			strm.Position = 0;
			Assert.Equal(b1, strm.ReadByte());
			Assert.Equal(b2, strm.ReadByte());
			Assert.Equal(b3, strm.ReadByte());
			Assert.Equal(b4, strm.ReadByte());
		}
	}

}