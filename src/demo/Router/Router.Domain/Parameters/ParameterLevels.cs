using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router.Domain.Parameters;

public enum ParameterLevels
{
	Invalid = 0,
	Users = 1,
	NetworkSupervisor = 2,
	Maintainer = 3,
	Manager = 3,
	Contractor = 4
}
