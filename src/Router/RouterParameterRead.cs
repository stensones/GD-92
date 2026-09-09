using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal sealed class RouterParameterRead
{
	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjectionSource? currentParameterSource;

	public RouterParameterRead(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource? currentParameterSource = null)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource;
	}

	public ValueTask<Envelope?> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not ParameterRequest
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} ||
			table != ParameterTable.Current ||
			number != RouterParameterCatalogue.BrigadeOrAgency.Number)
		{
			return ValueTask.FromResult<Envelope?>(null);
		}

		var brigadeOrAgencyIdentifier = this.currentParameterSource?.GetCurrent()
			.BrigadeOrAgencyIdentifier ?? this.localAddress.Brigade.Value;
		var response = Envelope.CreateParameterResponse(
			envelope,
			this.localAddress,
			this.protocolVersion,
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(brigadeOrAgencyIdentifier.ToWireValue())));

		return ValueTask.FromResult<Envelope?>(response);
	}
}
