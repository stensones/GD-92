using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed class InMemoryPendingDeliveryRegistry : IPendingDeliveryRegistry
{
	private const ushort MaximumSequenceNumber = 32767;
	private readonly object synchronizationLock = new();
	private readonly HashSet<UniqueSystemWideReference> pendingDeliveries = [];
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
					this.pendingDeliveries.Add(uswr);
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
			return this.pendingDeliveries.Contains(statusIdentifier.USWR);
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
