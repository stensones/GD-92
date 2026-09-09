using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal sealed class NodeLogin
{
	private static readonly PasswordLevel LevelZero =
		PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated);
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.Level1);

	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjectionSource currentParameterSource;

	public NodeLogin(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource ??
			throw new ArgumentNullException(nameof(currentParameterSource));
	}

	public ValueTask<Envelope?> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not SetParameter
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} setParameter ||
			table != ParameterTable.Current ||
			number != RouterParameterCatalogue.CurrentPassword.Number)
		{
			return ValueTask.FromResult<Envelope?>(null);
		}

		var valueBuffer = new EncodedMessageBuffer(setParameter.ParameterValue.ToWireValue());
		var submittedPassword = PasswordParameter.FromEncodedMessageBuffer(ref valueBuffer);

		if (submittedPassword.Level == LevelZero)
		{
			this.currentParameterSource.TryLogOffAtLevelZero(this.localAddress);
			return ValueTask.FromResult<Envelope?>(
				Envelope.CreateAcknowledgement(envelope, this.localAddress, this.protocolVersion));
		}

		if (submittedPassword.Level != LevelOne)
		{
			return ValueTask.FromResult<Envelope?>(null);
		}

		if (!this.currentParameterSource.TryLogOnAtLevelOne(submittedPassword))
		{
			return ValueTask.FromResult<Envelope?>(
				Envelope.CreateNegativeAcknowledgement(
					envelope,
					this.localAddress,
					this.protocolVersion,
					envelope.Destinations,
					ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidPassword)));
		}

		return ValueTask.FromResult<Envelope?>(
			Envelope.CreateAcknowledgement(envelope, this.localAddress, this.protocolVersion));
	}
}
