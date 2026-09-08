using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed class InMemoryManagementTransactionRegistry :
	IManagementTransactionRegistry
{
	private const ushort MaximumSequenceNumber = 32767;
	private readonly object synchronizationLock = new();
	private readonly Dictionary<UniqueSystemWideReference, RouterParameterRequestStatus> transactions = [];
	private readonly Dictionary<CommunicationsAddress, HashSet<ushort>> activeSequencesByDestination = [];
	private readonly Dictionary<CommunicationsAddress, ushort> nextSequenceByDestination = [];

	public RouterParameterRequestStatusIdentifier ReserveParameterRequest(
		CommunicationsAddress source,
		CommunicationsAddress destination)
	{
		return this.ReserveTransaction(
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

		return this.ReserveTransaction(
			source,
			destination,
			identifier => new PendingNodeLoginStatus(identifier, userAgentAddress));
	}

	public RouterParameterRequestStatusIdentifier ReserveNodeLogoff(
		CommunicationsAddress source,
		CommunicationsAddress destination)
	{
		return this.ReserveTransaction(
			source,
			destination,
			static identifier => new PendingNodeLogoffStatus(identifier));
	}

	private RouterParameterRequestStatusIdentifier ReserveTransaction(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		Func<RouterParameterRequestStatusIdentifier, RouterParameterRequestStatus> createPendingStatus)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);
		ArgumentNullException.ThrowIfNull(createPendingStatus);

		lock (this.synchronizationLock)
		{
			var activeSequences = this.GetActiveSequences(destination);
			var candidate = this.nextSequenceByDestination.GetValueOrDefault(destination);

			for (var attempts = 0; attempts <= MaximumSequenceNumber; attempts++)
			{
				if (!activeSequences.Contains(candidate))
				{
					var sequenceNumber = SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(candidate));
					var uswr = new UniqueSystemWideReference(source, destination, sequenceNumber);
					var identifier = new RouterParameterRequestStatusIdentifier(uswr);

					activeSequences.Add(candidate);
					this.transactions.Add(
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

	public bool IsActive(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.transactions.TryGetValue(statusIdentifier.USWR, out var status) &&
				status is PendingRouterParameterRequestStatus or DeferredRouterParameterRequestStatus or
					PendingNodeLoginStatus or PendingNodeLogoffStatus;
		}
	}

	public RouterParameterRequestStatus? GetStatus(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.transactions.GetValueOrDefault(statusIdentifier.USWR);
		}
	}

	public bool IsAwaitingFinalResponse(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.transactions.GetValueOrDefault(statusIdentifier.USWR) is
				DeferredRouterParameterRequestStatus;
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
			if (!this.transactions.TryGetValue(uswr, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					DeferredRouterParameterRequestStatus))
			{
				return false;
			}

			var parameter = (Parameter)envelope.Contents;
			this.transactions[uswr] = new ReceivedRouterParameterRequestStatus(
				new RouterParameterRequestStatusIdentifier(uswr),
				parameter.ParameterValue);
			this.GetActiveSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);

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
			if (!this.transactions.TryGetValue(uswr, out var status))
			{
				return false;
			}

			switch (status)
			{
				case PendingNodeLoginStatus pendingNodeLogin:
					this.transactions[uswr] = new LoggedOnNodeLoginStatus(
						new RouterParameterRequestStatusIdentifier(uswr),
						pendingNodeLogin.UserAgentAddress);
					break;
				case PendingNodeLogoffStatus:
					this.transactions[uswr] = new LoggedOffNodeLoginStatus(
						new RouterParameterRequestStatusIdentifier(uswr));
					break;
				default:
					return false;
			}

			this.GetActiveSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);

			return true;
		}
	}

	public bool TryCompleteNegativeAcknowledgement(Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not NegativeAcknowledgement negativeAcknowledgement ||
			envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested ||
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
			if (!this.transactions.TryGetValue(uswr, out var status))
			{
				return false;
			}

			switch (status)
			{
				case PendingNodeLoginStatus pendingNodeLogin:
					this.transactions[uswr] =
						negativeAcknowledgement.ReasonCode.ParameterReasonCode == ParameterReasonCode.InvalidPassword
							? new InvalidPasswordNodeLoginStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								pendingNodeLogin.UserAgentAddress)
							: new RejectedNodeLoginStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								pendingNodeLogin.UserAgentAddress);
					break;
				case PendingRouterParameterRequestStatus:
					this.transactions[uswr] =
						negativeAcknowledgement.ReasonCode.GeneralReasonCode ==
						GeneralReasonCode.WaitForAcknowledgement
							? new DeferredRouterParameterRequestStatus(
								new RouterParameterRequestStatusIdentifier(uswr))
							: new RejectedRouterParameterRequestStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								negativeAcknowledgement.ReasonCode);
					break;
				case PendingNodeLogoffStatus:
					this.transactions[uswr] = new RejectedRouterParameterRequestStatus(
						new RouterParameterRequestStatusIdentifier(uswr),
						negativeAcknowledgement.ReasonCode);
					break;
				default:
					return false;
			}

			this.GetActiveSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);

			return true;
		}
	}

	public bool TryTimeout(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			if (!this.transactions.TryGetValue(statusIdentifier.USWR, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					DeferredRouterParameterRequestStatus or PendingNodeLoginStatus or
					PendingNodeLogoffStatus))
			{
				return false;
			}

			this.transactions[statusIdentifier.USWR] = status switch
			{
				PendingNodeLoginStatus pendingNodeLogin => new TimedOutNodeLoginStatus(
					statusIdentifier,
					pendingNodeLogin.UserAgentAddress),
				_ => new TimedOutRouterParameterRequestStatus(statusIdentifier)
			};
			this.GetActiveSequences(statusIdentifier.USWR.Destination)
				.Remove(statusIdentifier.USWR.SequenceNumber.Value);
			return true;
		}
	}

	public bool TryRecordDeliveryFailure(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			if (!this.transactions.TryGetValue(statusIdentifier.USWR, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					PendingNodeLoginStatus or PendingNodeLogoffStatus))
			{
				return false;
			}

			this.transactions[statusIdentifier.USWR] =
				new DeliveryFailedRouterParameterRequestStatus(statusIdentifier);
			this.GetActiveSequences(statusIdentifier.USWR.Destination)
				.Remove(statusIdentifier.USWR.SequenceNumber.Value);
			return true;
		}
	}

	private HashSet<ushort> GetActiveSequences(CommunicationsAddress destination)
	{
		if (this.activeSequencesByDestination.TryGetValue(destination, out var activeSequences))
		{
			return activeSequences;
		}

		activeSequences = [];
		this.activeSequencesByDestination.Add(destination, activeSequences);
		return activeSequences;
	}
}
