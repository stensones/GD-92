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
		return this.Reserve(
			source,
			destination,
			static identifier => new PendingRouterParameterRequestStatus(identifier));
	}

	public RouterParameterRequestStatusIdentifier ReserveNodeLogin(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		CommunicationsAddress userAgentAddress)
	{
		ArgumentNullException.ThrowIfNull(userAgentAddress);

		return this.Reserve(
			source,
			destination,
			identifier => new PendingNodeLoginStatus(identifier, userAgentAddress));
	}

	private RouterParameterRequestStatusIdentifier Reserve(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		Func<RouterParameterRequestStatusIdentifier, RouterParameterRequestStatus> createPendingStatus)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);
		ArgumentNullException.ThrowIfNull(createPendingStatus);

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
					var identifier = new RouterParameterRequestStatusIdentifier(uswr);

					pendingSequences.Add(candidate);
					this.deliveries.Add(
						uswr,
						createPendingStatus(identifier));
					this.nextSequenceByDestination[destination] =
						candidate == MaximumSequenceNumber ? (ushort)0 : (ushort)(candidate + 1);

					return identifier;
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
				status is PendingRouterParameterRequestStatus or PendingNodeLoginStatus;
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

	public bool TryCompleteAcknowledgement(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not Acknowledgement ||
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
				status is not PendingNodeLoginStatus pendingNodeLogin)
			{
				return false;
			}

			this.deliveries[uswr] = new LoggedOnNodeLoginStatus(
				new RouterParameterRequestStatusIdentifier(uswr),
				pendingNodeLogin.UserAgentAddress);
			this.GetPendingSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);

			return true;
		}
	}

	public bool TryCompleteInvalidPasswordNegativeAcknowledgement(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not NegativeAcknowledgement negativeAcknowledgement ||
			negativeAcknowledgement.ReasonCode.ParameterReasonCode != ParameterReasonCode.InvalidPassword ||
			envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var uswr = new UniqueSystemWideReference(
			envelope.Destinations.Addresses[0],
			envelope.Source,
			envelope.AcknowledgementAndSequence.SequenceNumber);

		if (negativeAcknowledgement.Destinations.Addresses.Count != 1 ||
			negativeAcknowledgement.Destinations.Addresses[0] != uswr.Destination)
		{
			return false;
		}

		lock (this.synchronizationLock)
		{
			if (!this.deliveries.TryGetValue(uswr, out var status) ||
				status is not PendingNodeLoginStatus pendingNodeLogin)
			{
				return false;
			}

			this.deliveries[uswr] = new InvalidPasswordNodeLoginStatus(
				new RouterParameterRequestStatusIdentifier(uswr),
				pendingNodeLogin.UserAgentAddress);
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
