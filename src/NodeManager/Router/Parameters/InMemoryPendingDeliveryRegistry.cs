using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed class InMemoryPendingDeliveryRegistry : IPendingDeliveryRegistry
{
	private const ushort MaximumSequenceNumber = 32767;
	private readonly object synchronizationLock = new();
	private readonly Dictionary<UniqueSystemWideReference, RouterParameterRequestStatus> deliveries = [];
	private readonly Dictionary<CommunicationsAddress, HashSet<ushort>> pendingSequencesByDestination = [];
	private readonly Dictionary<CommunicationsAddress, ushort> nextSequenceByDestination = [];

	public RouterParameterRequestStatusIdentifier Reserve(
		CommunicationsAddress source,
		CommunicationsAddress destination)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);

		lock (this.synchronizationLock)
		{
			var pendingSequences = this.GetPendingSequences(destination);
			var candidate = this.nextSequenceByDestination.GetValueOrDefault(destination);

			for (var attempts = 0; attempts <= MaximumSequenceNumber; attempts++)
			{
				if (!pendingSequences.Contains(candidate))
				{
					var sequenceNumber = SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(candidate));
					var uswr = new UniqueSystemWideReference(source, destination, sequenceNumber);

					pendingSequences.Add(candidate);
					this.deliveries.Add(
						uswr,
						new PendingRouterParameterRequestStatus(
							new RouterParameterRequestStatusIdentifier(uswr)));
					this.nextSequenceByDestination[destination] =
						candidate == MaximumSequenceNumber ? (ushort)0 : (ushort)(candidate + 1);

					return new RouterParameterRequestStatusIdentifier(uswr);
				}

				candidate = candidate == MaximumSequenceNumber ? (ushort)0 : (ushort)(candidate + 1);
			}
		}

		throw new InvalidOperationException("All sequence numbers for the destination are pending.");
	}

	public bool IsPending(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.deliveries.TryGetValue(statusIdentifier.USWR, out var status) &&
				status is PendingRouterParameterRequestStatus;
		}
	}

	public RouterParameterRequestStatus? GetStatus(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.deliveries.GetValueOrDefault(statusIdentifier.USWR);
		}
	}

	public bool TryCompleteParameterResponse(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not Parameter ||
			envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var uswr = new UniqueSystemWideReference(
			envelope.Destinations.Addresses[0],
			envelope.Source,
			envelope.AcknowledgementAndSequence.SequenceNumber);

		lock (this.synchronizationLock)
		{
			if (!this.deliveries.TryGetValue(uswr, out var status) ||
				status is not PendingRouterParameterRequestStatus)
			{
				return false;
			}

			var parameter = (Parameter)envelope.Contents;
			this.deliveries[uswr] = new ReceivedRouterParameterRequestStatus(
				new RouterParameterRequestStatusIdentifier(uswr),
				parameter.ParameterValue);
			this.GetPendingSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);

			return true;
		}
	}

	private HashSet<ushort> GetPendingSequences(CommunicationsAddress destination)
	{
		if (this.pendingSequencesByDestination.TryGetValue(destination, out var pendingSequences))
		{
			return pendingSequences;
		}

		pendingSequences = [];
		this.pendingSequencesByDestination.Add(destination, pendingSequences);
		return pendingSequences;
	}
}
