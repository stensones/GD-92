using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal enum LocalDeliveryOutcome
{
	RouterHandling,
	LocalParticipantIngress,
	UserAgentIngress,
	NotLocallyDeliverable
}

internal sealed class LocalDeliveryClassification
{
	private readonly CommunicationsAddress localRouter;

	public LocalDeliveryClassification(CommunicationsAddress localRouter)
	{
		ArgumentNullException.ThrowIfNull(localRouter);

		this.localRouter = localRouter;
	}

	public LocalDeliveryOutcome Classify(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Destinations.Addresses.Count != 1)
		{
			return LocalDeliveryOutcome.NotLocallyDeliverable;
		}

		var destination = envelope.Destinations.Addresses[0];

		if (destination.Brigade != this.localRouter.Brigade ||
			destination.Node != this.localRouter.Node)
		{
			return LocalDeliveryOutcome.NotLocallyDeliverable;
		}

		if (destination.Port == this.localRouter.Port)
		{
			return LocalDeliveryOutcome.RouterHandling;
		}

		if (destination.Port.Value != 0 &&
			envelope.Contents is ParameterRequest or ParameterRequestMultiple or SetParameter)
		{
			return LocalDeliveryOutcome.LocalParticipantIngress;
		}

		if (destination.Port.Value != 0)
		{
			return LocalDeliveryOutcome.UserAgentIngress;
		}

		return LocalDeliveryOutcome.NotLocallyDeliverable;
	}
}
