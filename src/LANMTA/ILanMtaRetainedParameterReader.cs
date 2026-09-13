using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA;

public interface ILanMtaRetainedParameterReader
{
	ValueTask<ParameterValue?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken);
}
