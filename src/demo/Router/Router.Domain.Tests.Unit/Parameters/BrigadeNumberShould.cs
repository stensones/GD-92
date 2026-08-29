using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;
using Router.Domain.Parameters;

namespace Router.Domain.Tests.Unit.Parameters;

public class BrigadeNumberShould
{
	[Fact]
	public void	Have_Correct_ParameterNumber()
	{
		var brigadeNumber = new BrigadeNumber(new Word8(44));
		Assert.Equal<ParameterNumbers>(ParameterNumbers.BrigadeNumber, brigadeNumber.ParameterNumber);
	}
}
