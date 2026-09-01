namespace Router.bob;

using System;
using System.Collections.Generic;
using System.Text;

public class GD92MessageHandler
{
	public void Handle(GD92Message message)
	{
		Console.WriteLine($"Received GD92 message with payload: {BitConverter.ToString(message.payload)}");
	}
}
