using Router.Persistence;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal sealed class RouterParameterRead
{
	private static readonly Password RedactedPassword = Password.FromValue(
		PasswordValue.FromValue(SevenBitAsciiString.FromValue("PASSWORD")));
	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjectionSource? currentParameterSource;
	private readonly IParticipantParameterStore? parameterStore;
	private readonly NodeName? nodeName;

	public RouterParameterRead(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource? currentParameterSource = null,
		IParticipantParameterStore? parameterStore = null,
		NodeName? nodeName = null)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource;
		this.parameterStore = parameterStore;
		this.nodeName = nodeName;
	}

	public async ValueTask<Envelope?> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is ParameterRequest
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} &&
			table == ParameterTable.Current)
		{
			var parameterValue = RouterParameterCatalogue.IsPasswordNumber(number)
				? ParameterValue.FromWireValue(RedactedPassword.ToWireValue())
				: this.CurrentParameterValue(number);

			if (parameterValue is null)
			{
				if (number.Value is < 1 or > 21)
				{
					return Envelope.CreateNegativeAcknowledgement(
						envelope,
						this.localAddress,
						this.protocolVersion,
						envelope.Destinations,
						ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidParameter));
				}

				return null;
			}

			return Envelope.CreateParameterResponse(
				envelope,
				this.localAddress,
				this.protocolVersion,
				Parameter.FromFields(
					MoreValues.No,
					parameterValue));
		}

		if (envelope.Contents is not ParameterRequestMultiple
			{
				ParameterTable: var parameterTable,
				ParameterNumber: var parameterNumber
			} parameterRequest ||
			parameterTable != ParameterTable.Current ||
			parameterNumber != RouterParameterCatalogue.RouterTable.Number ||
			this.parameterStore is null)
		{
			return null;
		}

		var routingTableParameterValue = await this.parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.RouterTable.Number,
			cancellationToken);
		if (routingTableParameterValue is null)
		{
			return null;
		}

		var routingTable = RouterParameterCatalogue.RouterTable.Read(routingTableParameterValue);
		var selectionBuffer = new EncodedMessageBuffer(parameterRequest.EntrySelection.ToWireValue());
		var firstEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref selectionBuffer);
		var lastEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref selectionBuffer);
		if (firstEntry.Value == 0)
		{
			return null;
		}

		var entries = routingTable.Entries
			.Where(entry => entry.Index.Value >= firstEntry.Value &&
				entry.Index.Value <= lastEntry.Value)
			.OrderBy(entry => entry.Index.Value)
			.ToArray();
		var requestedEntryCount = lastEntry.Value - firstEntry.Value + 1;
		if (entries.Length != requestedEntryCount)
		{
			return Envelope.CreateNegativeAcknowledgement(
				envelope,
				this.localAddress,
				this.protocolVersion,
				envelope.Destinations,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidEntry));
		}

		var responseEntries = new List<RoutingTableEntry>();
		foreach (var entry in entries)
		{
			var candidateValue = RouterParameterCatalogue.RouterTable.Encode(
				RoutingTable.FromEntries([.. responseEntries, entry]));
			if (Parameter.FromFields(MoreValues.No, candidateValue).ToWireValue().Length >
				Envelope.MaximumContentsLength)
			{
				break;
			}

			responseEntries.Add(entry);
		}

		if (responseEntries.Count == 0)
		{
			throw new InvalidOperationException(
				"A Routing Table entry cannot fit in a GD-92 Parameter response.");
		}

		var moreValues = responseEntries.Count < entries.Length
			? MoreValues.Yes
			: MoreValues.No;

		return Envelope.CreateParameterResponse(
			envelope,
			this.localAddress,
			this.protocolVersion,
			Parameter.FromFields(
				moreValues,
				RouterParameterCatalogue.RouterTable.Encode(
					RoutingTable.FromEntries([.. responseEntries]))));
	}

	private ParameterValue? CurrentParameterValue(ParameterNumber number)
	{
		var currentParameters = this.currentParameterSource?.GetCurrent();

		return number == RouterParameterCatalogue.BrigadeOrAgency.Number
			? RouterParameterCatalogue.BrigadeOrAgency.Encode(
				currentParameters?.BrigadeOrAgencyIdentifier ?? this.localAddress.Brigade.Value)
			: number == RouterParameterCatalogue.NodeNumber.Number
				? RouterParameterCatalogue.NodeNumber.Encode(this.localAddress.Node.Value)
			: number == RouterParameterCatalogue.NodeName.Number
				? this.nodeName is null
					? null
					: RouterParameterCatalogue.NodeName.Encode(this.nodeName)
			: number == RouterParameterCatalogue.CurrentPassword.Number
				? RouterParameterCatalogue.CurrentPassword.Encode(
					PasswordParameter.FromFields(
						currentParameters?.CurrentPassword.Level ??
							PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
						RedactedPassword,
						currentParameters?.CurrentPassword.CommunicationsAddress ?? this.localAddress))
				: number == RouterParameterCatalogue.NoAcknowledgementTimeout.Number
					? currentParameters is null
						? null
						: RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
							currentParameters.NoAcknowledgementTimeout)
				: number == RouterParameterCatalogue.Retries.Number
					? currentParameters is null
						? null
						: RouterParameterCatalogue.Retries.Encode(currentParameters.Retries)
				: null;
	}
}
