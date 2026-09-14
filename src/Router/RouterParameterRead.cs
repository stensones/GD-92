using Router.Persistence;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using System.Globalization;

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
	private readonly MaximumMessageLength? maximumMessageLength;
	private readonly CommunicationsAddress? networkManagerAddress1;
	private readonly CommunicationsAddress? networkManagerAddress2;
	private readonly ManualAcknowledgementTimeout? manualAcknowledgementTimeout;
	private readonly TimeProvider timeProvider;

	public RouterParameterRead(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource? currentParameterSource = null,
		IParticipantParameterStore? parameterStore = null,
		NodeName? nodeName = null,
		MaximumMessageLength? maximumMessageLength = null,
		CommunicationsAddress? networkManagerAddress1 = null,
		CommunicationsAddress? networkManagerAddress2 = null,
		ManualAcknowledgementTimeout? manualAcknowledgementTimeout = null,
		TimeProvider? timeProvider = null)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource;
		this.parameterStore = parameterStore;
		this.nodeName = nodeName;
		this.maximumMessageLength = maximumMessageLength;
		this.networkManagerAddress1 = networkManagerAddress1;
		this.networkManagerAddress2 = networkManagerAddress2;
		this.manualAcknowledgementTimeout = manualAcknowledgementTimeout;
		this.timeProvider = timeProvider ?? TimeProvider.System;
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
			(parameterNumber != RouterParameterCatalogue.RouterTable.Number &&
				parameterNumber != RouterParameterCatalogue.PstnTable.Number &&
				parameterNumber != RouterParameterCatalogue.WanTable.Number &&
				parameterNumber != RouterParameterCatalogue.LanTable.Number &&
				parameterNumber != RouterParameterCatalogue.IsdnTable.Number) ||
			this.parameterStore is null)
		{
			return null;
		}

		var tableParameterValue = await this.parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			parameterNumber,
			cancellationToken);
		if (tableParameterValue is null)
		{
			return null;
		}

		var selectionBuffer = new EncodedMessageBuffer(parameterRequest.EntrySelection.ToWireValue());
		var firstEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref selectionBuffer);
		var lastEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref selectionBuffer);
		if (firstEntry.Value == 0)
		{
			return null;
		}

		if (parameterNumber == RouterParameterCatalogue.RouterTable.Number)
		{
			return this.CreateTableResponse(
				envelope,
				RouterParameterCatalogue.RouterTable.Read(tableParameterValue).Entries,
				firstEntry,
				lastEntry,
				static entry => entry.Index.Value,
				static entries => RouterParameterCatalogue.RouterTable.Encode(
					RoutingTable.FromEntries(entries)),
				"Routing Table");
		}

		if (parameterNumber == RouterParameterCatalogue.PstnTable.Number)
		{
			return this.CreateTableResponse(
				envelope,
				RouterParameterCatalogue.PstnTable.Read(tableParameterValue).Entries,
				firstEntry,
				lastEntry,
				static entry => entry.Index.Value,
				static entries => RouterParameterCatalogue.PstnTable.Encode(
					PstnTable.FromEntries(entries)),
				"PSTN Table");
		}

		if (parameterNumber == RouterParameterCatalogue.WanTable.Number)
		{
			return this.CreateTableResponse(
				envelope,
				RouterParameterCatalogue.WanTable.Read(tableParameterValue).Entries,
				firstEntry,
				lastEntry,
				static entry => entry.Index.Value,
				static entries => RouterParameterCatalogue.WanTable.Encode(
					WanTable.FromEntries(entries)),
				"WAN Table");
		}

		if (parameterNumber == RouterParameterCatalogue.LanTable.Number)
		{
			return this.CreateTableResponse(
				envelope,
				RouterParameterCatalogue.LanTable.Read(tableParameterValue).Entries,
				firstEntry,
				lastEntry,
				static entry => entry.Index.Value,
				static entries => RouterParameterCatalogue.LanTable.Encode(
					LanTable.FromEntries(entries)),
				"LAN Table");
		}

		return this.CreateTableResponse(
			envelope,
			RouterParameterCatalogue.IsdnTable.Read(tableParameterValue).Entries,
			firstEntry,
			lastEntry,
			static entry => entry.Index.Value,
			static entries => RouterParameterCatalogue.IsdnTable.Encode(
				IsdnTable.FromEntries(entries)),
			"ISDN Table");
	}

	private Envelope CreateTableResponse<TEntry>(
		Envelope request,
		IReadOnlyList<TEntry> tableEntries,
		ParameterEntryIndex firstEntry,
		ParameterEntryIndex lastEntry,
		Func<TEntry, ushort> index,
		Func<TEntry[], ParameterValue> encode,
		string tableName)
	{
		var entries = tableEntries
			.Where(entry => index(entry) >= firstEntry.Value &&
				index(entry) <= lastEntry.Value)
			.OrderBy(index)
			.ToArray();
		var requestedEntryCount = (int)lastEntry.Value - firstEntry.Value + 1;
		if (entries.Length != requestedEntryCount)
		{
			return Envelope.CreateNegativeAcknowledgement(
				request,
				this.localAddress,
				this.protocolVersion,
				request.Destinations,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidEntry));
		}

		var responseEntries = new List<TEntry>();
		foreach (var entry in entries)
		{
			var candidateValue = encode([.. responseEntries, entry]);
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
				$"A {tableName} entry cannot fit in a GD-92 Parameter response.");
		}

		var moreValues = responseEntries.Count < entries.Length
			? MoreValues.Yes
			: MoreValues.No;

		return Envelope.CreateParameterResponse(
			request,
			this.localAddress,
			this.protocolVersion,
			Parameter.FromFields(
				moreValues,
				encode([.. responseEntries])));
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
			: number == RouterParameterCatalogue.MaximumMessageLength.Number
				? this.maximumMessageLength is null
					? null
					: RouterParameterCatalogue.MaximumMessageLength.Encode(this.maximumMessageLength)
			: number == RouterParameterCatalogue.NetworkManagerAddress1.Number
				? this.networkManagerAddress1 is null
					? null
					: RouterParameterCatalogue.NetworkManagerAddress1.Encode(this.networkManagerAddress1)
			: number == RouterParameterCatalogue.NetworkManagerAddress2.Number
				? this.networkManagerAddress2 is null
					? null
					: RouterParameterCatalogue.NetworkManagerAddress2.Encode(this.networkManagerAddress2)
			: number == RouterParameterCatalogue.ManualAcknowledgementTimeout.Number
				? this.manualAcknowledgementTimeout is null
					? null
					: RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
						this.manualAcknowledgementTimeout)
			: number == RouterParameterCatalogue.TimeAndDate.Number
				? RouterParameterCatalogue.TimeAndDate.Encode(TimeAndDate.FromValue(
					SevenBitAsciiString.FromValue(this.timeProvider.GetUtcNow().ToString(
						"ddMMMyyHHmmss",
						CultureInfo.InvariantCulture).ToUpperInvariant())))
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
