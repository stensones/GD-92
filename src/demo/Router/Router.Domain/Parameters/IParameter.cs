using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Router.Domain.Fields;

namespace Router.Domain.Parameters;

public interface IParameter<T> where T : IField
{
	ParameterNumbers ParameterNumber { get; }

	ParameterLevels Level { get; }

	T Value { get; set; }
}
