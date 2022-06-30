using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stensones.GD92.Fields.Derived;

namespace Stensones.GD92.Fields.Tests.Unit.Derived;
public class ActiveStateShould
{
	[Fact]
	public void Be_Of_Type_Word8()
	{
		var sut = new ActiveState(true);
		Assert.IsAssignableFrom<Word8>(sut);
	}

	//[Fact]
	//public void Be_Of_Type_Word8()
	//{
	//	var sut = new ActiveState(false);
	//	Assert.True(sut.Value);
	//}
}
