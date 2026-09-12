using ParticipantParameters;
using Stensones.GD92.Messages;

namespace LANMTA.Persistence;

public sealed class LanMtaParameterBootstrapper
{
	private readonly ParticipantParameterBootstrapper parameterBootstrapper;

	public LanMtaParameterBootstrapper(IParticipantParameterStore store)
	{
		this.parameterBootstrapper = new ParticipantParameterBootstrapper(store);
	}

	public async ValueTask<LanMtaCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		LanMtaParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		return await this.parameterBootstrapper.InitializeAsync(
		[
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.PortNumber.Number,
				LanMtaParameterCatalogue.PortNumber.Encode(configuration.PortNumber)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.AgentType.Number,
				LanMtaParameterCatalogue.AgentType.Encode(configuration.AgentType)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.InterfaceStatus.Number,
				LanMtaParameterCatalogue.InterfaceStatus.Encode(
					LanMtaParameterCatalogue.IdleInterfaceStatus)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.NotifyStatusChanges.Number,
				LanMtaParameterCatalogue.NotifyStatusChanges.Encode(
					LanMtaParameterCatalogue.NotifyStatusChangesDefault)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.FrameTransmitCount.Number,
				LanMtaParameterCatalogue.FrameTransmitCount.Encode(
					LanMtaParameterCatalogue.InitialFrameTransmitCount)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.FrameReceiveCount.Number,
				LanMtaParameterCatalogue.FrameReceiveCount.Encode(
					LanMtaParameterCatalogue.InitialFrameReceiveCount)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.FrameTransmitFailureCount.Number,
				LanMtaParameterCatalogue.FrameTransmitFailureCount.Encode(
					LanMtaParameterCatalogue.InitialFrameTransmitFailureCount)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.FrameReceiveFailureCount.Number,
				LanMtaParameterCatalogue.FrameReceiveFailureCount.Encode(
					LanMtaParameterCatalogue.InitialFrameReceiveFailureCount)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.Priority.Number,
				LanMtaParameterCatalogue.Priority.Encode(
					LanMtaParameterCatalogue.StandardPriority)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.NextNodes.Number,
				LanMtaParameterCatalogue.NextNodes.Encode(
					LanMtaParameterCatalogue.EmptyNextNodes)),
			ParameterBootstrapValue.FromValues(
				LanMtaParameterCatalogue.MyLanAddress.Number,
				LanMtaParameterCatalogue.MyLanAddress.Encode(
					LanMtaParameterCatalogue.StationEndLanAddress))
		],
		static (nonVolatileValues, _) => ValueTask.FromResult(
			LanMtaCurrentParameterProjection.FromNonVolatileParameters(nonVolatileValues)),
		cancellationToken);
	}
}
