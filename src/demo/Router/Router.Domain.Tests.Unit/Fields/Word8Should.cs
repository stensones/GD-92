using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;

namespace Router.Domain.Tests.Unit.Fields;

public class Word8Should
{
	[Fact]
	public void Be_An_IField()
	{
		var sut = new Word8(1);
		Assert.IsAssignableFrom<IField>(sut);
	}
}
