using Router.Persistence;
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
			table != ParameterTable.Current)
		{
			return ValueTask.FromResult<Envelope?>(null);
		}

		var parameterValue = number == RouterParameterCatalogue.Level1PasswordNumber
			? ParameterValue.FromWireValue(RedactedPassword.ToWireValue())
			: this.CurrentParameterValue(number);

		if (parameterValue is null)
		{
			return ValueTask.FromResult<Envelope?>(null);
		}

		var response = Envelope.CreateParameterResponse(
			envelope,
			this.localAddress,
			this.protocolVersion,
			Parameter.FromFields(
				MoreValues.No,
				parameterValue));

		return ValueTask.FromResult<Envelope?>(response);
	}

	private ParameterValue? CurrentParameterValue(ParameterNumber number)
	{
		var currentParameters = this.currentParameterSource?.GetCurrent();

		return number == RouterParameterCatalogue.BrigadeOrAgency.Number
			? RouterParameterCatalogue.BrigadeOrAgency.Encode(
				currentParameters?.BrigadeOrAgencyIdentifier ?? this.localAddress.Brigade.Value)
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
