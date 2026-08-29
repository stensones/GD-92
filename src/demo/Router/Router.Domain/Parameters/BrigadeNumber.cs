using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;

namespace Router.Domain.Parameters;

public class BrigadeNumber : IParameter<Word8>
{
	public ParameterNumbers ParameterNumber { get; private set; }
	public ParameterLevels Level { get; }
	public Word8 Value { get; set; }

	public BrigadeNumber(Word8 brigade)
	{
		this.ParameterNumber = ParameterNumbers.BrigadeNumber;
		this.Level = ParameterLevels.NetworkSupervisor;
		this.Value = brigade;
	}
}
