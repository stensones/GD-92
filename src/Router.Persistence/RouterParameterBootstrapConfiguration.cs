using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapConfiguration
{
	private RouterParameterBootstrapConfiguration(
		CommunicationsAddress localAddress,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		this.LocalAddress = localAddress;
		this.InitialLevel1Password = initialLevel1Password;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
	}

	public CommunicationsAddress LocalAddress { get; }
	public PasswordValue InitialLevel1Password { get; }
	public NoAcknowledgementTimeout NoAcknowledgementTimeout { get; }
	public Retries Retries { get; }

	public static RouterParameterBootstrapConfiguration FromValues(
		CommunicationsAddress localAddress,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		ArgumentNullException.ThrowIfNull(localAddress);
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(retries);

		return new RouterParameterBootstrapConfiguration(
			localAddress,
			initialLevel1Password,
			noAcknowledgementTimeout,
			retries);
	}
}
