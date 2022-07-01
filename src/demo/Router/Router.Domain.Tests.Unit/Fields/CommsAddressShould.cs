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
	public void Set_BrigadeNodeAndPort_WhenConstructedViaParts()
	{
		const byte Brigade = 26;
		const ushort Node = 123;
		const byte Port = 23;

		var sut = new CommsAddress(Brigade, Node, Port);
		Assert.Equal(Brigade, sut.Brigade);
		Assert.Equal(Node, sut.Node);
		Assert.Equal(Port, sut.Port);
	}

	[Fact]
	public void Set_BrigadeNodeAndPort_WhenConstructedViaString()
	{
		const byte Brigade = 26;
		const ushort Node = 234;
		const byte Port = 23;

		var sut = new CommsAddress($"{Brigade}:{Node}:{Port}");
		Assert.Equal(Brigade, sut.Brigade);
		Assert.Equal(Node, sut.Node);
		Assert.Equal(Port, sut.Port);
	}

	[Fact]
	public void Throw_WhenConstructorString_IsInvalid()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("sdff"));
	}

	[Fact]
	public void Throw_WhenConstructorString_IsEmpty()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress(string.Empty));
	}

	//bits 0-7 brigade_identifier(0-255)
	[Fact]
	public void Throw_WhenConstructorString_BrigadeIsTooBig()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("256:3:2"));
	}

	//bits 8-17 node_identifier(0-1023)
	[Fact]
	public void Throw_WhenConstructorString_NodeIsTooBig()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("4:1024:2"));
	}

	//bits 18-23 port_identifier(0-63)
	[Fact]
	public void Throw_WhenConstructorStrings_PortIsTooBig()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CommsAddress("34:3:64"));
	}
}
