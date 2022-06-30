using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;

namespace Router.Domain.Tests.Unit.Fields;

public class CommsAddressShould
{
	[Fact]
	public void SetBrigadeNodeAndPortWhenConstructedViaParts()
	{
		const byte Brigade = 26;
		const ushort Node = 1234;
		const byte Port = 23;

		var sut = new CommsAddress(Brigade, Node, Port);
		Assert.Equal(Brigade, sut.Brigade);
		Assert.Equal(Node, sut.Node);
		Assert.Equal(Port, sut.Port);
	}

	[Fact]
	public void SetBrigadeNodeAndPortWhenConstructedViaString()
	{
		const byte Brigade = 26;
		const ushort Node = 1234;
		const byte Port = 23;

		var sut = new CommsAddress($"{Brigade}:{Node}:{Port}");
		Assert.Equal(Brigade, sut.Brigade);
		Assert.Equal(Node, sut.Node);
		Assert.Equal(Port, sut.Port);
	}

	[Fact]
	public void ThrowWhenConstructorStringIsInvalid()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("sdff"));
	}

	[Fact]
	public void ThrowWhenConstructorStringIsEmpty()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress(string.Empty));
	}

	[Fact]
	public void ThrowWhenConstructorStringsBrigadeIsTooBig()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("1234:3:2"));
	}

	[Fact]
	public void ThrowWhenConstructorStringsPortIsTooBig()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("34:3:2321"));
	}
}
