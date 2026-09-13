using Microsoft.Extensions.DependencyInjection;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA;

public sealed class LanMtaRetainedParameterReader(
	IServiceScopeFactory serviceScopeFactory) : ILanMtaRetainedParameterReader
{
	public async ValueTask<ParameterValue?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		await using var scope = serviceScopeFactory.CreateAsyncScope();
		var parameterStore = scope.ServiceProvider.GetRequiredService<IParticipantParameterStore>();
		return await parameterStore.GetAsync(
			parameterTable,
			parameterNumber,
			cancellationToken);
	}
}
