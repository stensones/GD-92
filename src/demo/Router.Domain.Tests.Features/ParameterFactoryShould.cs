using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;
using Router.Domain.Parameters;

namespace Router.Domain.Tests.Features;

public class ParameterFactoryShould
{
	[Fact]
	public void CreateParameter_MatchingName_WithSpecifiedValue()
	{
		var parameter = ParameterFactory.Create<byte>("brigade_number", new Word8(10));

		Assert.NotNull(parameter);
		Assert.IsAssignableFrom<IParameter<Word8>>(parameter);
		Assert.Equal(ParameterNumbers.BrigadeNumber, parameter.ParameterNumber);
	}
}
