using Router.Persistence;
using Stensones.GD92.Fields;

namespace Router.Tests.Unit;

internal static class RouterParameterRequestHandlerFactory
{
	public static RouterParameterRequestHandler Create(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion)
	{
		var currentParameterSource = new RouterCurrentParameterProjectionSource();

		return new RouterParameterRequestHandler(
			localAddress,
			new RouterParameterRead(localAddress, protocolVersion),
			new NodeLogin(localAddress, protocolVersion, currentParameterSource),
			new Level1PasswordModification(
				localAddress,
				protocolVersion,
				currentParameterSource,
				new UnsupportedPasswordVerifierStore()));
	}

	public static RouterParameterRequestHandler Create(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjection currentParameters)
	{
		var currentParameterSource = new RouterCurrentParameterProjectionSource();
		currentParameterSource.Publish(currentParameters);

		return Create(
			localAddress,
			protocolVersion,
			currentParameterSource,
			new UnsupportedPasswordVerifierStore());
	}

	public static RouterParameterRequestHandler Create(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource)
	{
		return Create(
			localAddress,
			protocolVersion,
			currentParameterSource,
			new UnsupportedPasswordVerifierStore());
	}

	public static RouterParameterRequestHandler Create(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		RouterCurrentParameterProjectionSource currentParameterSource,
		IRouterLevel1PasswordVerifierStore passwordVerifierStore)
	{
		return new RouterParameterRequestHandler(
			localAddress,
			new RouterParameterRead(localAddress, protocolVersion, currentParameterSource),
			new NodeLogin(localAddress, protocolVersion, currentParameterSource),
			new Level1PasswordModification(
				localAddress,
				protocolVersion,
				currentParameterSource,
				passwordVerifierStore));
	}

	private sealed class UnsupportedPasswordVerifierStore : IRouterLevel1PasswordVerifierStore
	{
		public ValueTask<PasswordVerifier?> GetAsync(
			ParameterTable parameterTable,
			CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			PasswordVerifier passwordVerifier,
			CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}
	}
}
