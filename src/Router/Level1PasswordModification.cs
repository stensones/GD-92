using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal sealed class Level1PasswordModification
{
	private readonly CommunicationsAddress localAddress;
	private readonly ProtocolVersion protocolVersion;
	private readonly RouterCurrentParameterProjectionSource currentParameterSource;
	private readonly IRouterPasswordVerifierStore passwordVerifierStore;

	public Level1PasswordModification(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource,
		IRouterPasswordVerifierStore passwordVerifierStore)
	{
		this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
		this.protocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
		this.currentParameterSource = currentParameterSource ??
			throw new ArgumentNullException(nameof(currentParameterSource));
		this.passwordVerifierStore = passwordVerifierStore ??
			throw new ArgumentNullException(nameof(passwordVerifierStore));
	}

	public async ValueTask<Envelope?> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not SetParameter
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} setParameter ||
			number != RouterParameterCatalogue.Level1PasswordNumber)
		{
			return null;
		}

		if (table == ParameterTable.Permanent)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		if (table != ParameterTable.Current && table != ParameterTable.NonVolatile)
		{
			return null;
		}

		if (!this.currentParameterSource.HasActiveNodeLoginAtLevelOne())
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		var valueBuffer = new EncodedMessageBuffer(setParameter.ParameterValue.ToWireValue());
		var password = Password.FromEncodedMessageBuffer(ref valueBuffer);
		if (valueBuffer.RemainingBitCount != 0)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		var passwordVerifier = PasswordVerifier.Create(
			password.Value,
			PasswordVerifierWorkFactor.Default);
		if (table == ParameterTable.NonVolatile)
		{
			await this.passwordVerifierStore.StoreAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.Level1PasswordNumber,
				passwordVerifier,
				cancellationToken);
		}

		if (table == ParameterTable.Current &&
			!this.currentParameterSource.TryChangeLevel1Password(passwordVerifier))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, this.localAddress, this.protocolVersion);
	}

	private Envelope CreateNegativeAcknowledgement(
		Envelope envelope,
		ParameterReasonCode reasonCode)
	{
		return Envelope.CreateNegativeAcknowledgement(
			envelope,
			this.localAddress,
			this.protocolVersion,
			envelope.Destinations,
			ReasonCode.FromParameterReasonCode(reasonCode));
	}
}
