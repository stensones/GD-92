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
	public void Inherit_From_Field_Of_UShort()
	{
		var sut = new Word16(123);
		Assert.IsAssignableFrom<Field<ushort>>(sut);
	}

	[Fact]
	public void Have_Value_That_Is_Unsigned_16Bit()
	{
		var sut = new Word16(0);
		Assert.IsAssignableFrom<ushort>(sut.Value);
	}

	[Fact]
	public void Write_2_Bytes_To_Stream_When_Serialised()
	{
		const byte inptValue = 9;
		var sut = new Word16(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			Assert.Equal(2, strm.Length);
			Assert.Equal(2, strm.Position);
		}
	}

	[Fact]
	public void Have_Field_Value_In_First_Stream_Byte_When_Serialised()
	{
		const byte inptValue = 9;
		const byte msb = 0x00;
		const byte lsb = 0x09;
		var sut = new Word16(inptValue);
		using (var strm = new MemoryStream())
		{
			sut.WriteToStream(strm);

			strm.Flush();
			strm.Position = 0;
			Assert.Equal(msb, strm.ReadByte());
			Assert.Equal(lsb, strm.ReadByte());
		}
	}
}