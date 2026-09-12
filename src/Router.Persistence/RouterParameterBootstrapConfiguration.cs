using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapConfiguration
{
	private RouterParameterBootstrapConfiguration(
		CommunicationsAddress localAddress,
		NodeName nodeName,
		MaximumMessageLength maximumMessageLength,
		CommunicationsAddress networkManagerAddress1,
		CommunicationsAddress networkManagerAddress2,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries,
		ManualAcknowledgementTimeout manualAcknowledgementTimeout,
		TimeAndDate timeAndDate,
		MdtTable mdtTable)
	{
		this.LocalAddress = localAddress;
		this.NodeName = nodeName;
		this.MaximumMessageLength = maximumMessageLength;
		this.NetworkManagerAddress1 = networkManagerAddress1;
		this.NetworkManagerAddress2 = networkManagerAddress2;
		this.InitialLevel1Password = initialLevel1Password;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
		this.ManualAcknowledgementTimeout = manualAcknowledgementTimeout;
		this.TimeAndDate = timeAndDate;
		this.MdtTable = mdtTable;
	}

	public CommunicationsAddress LocalAddress { get; }
	public NodeName NodeName { get; }
	public MaximumMessageLength MaximumMessageLength { get; }
	public CommunicationsAddress NetworkManagerAddress1 { get; }
	public CommunicationsAddress NetworkManagerAddress2 { get; }
	public PasswordValue InitialLevel1Password { get; }
	public NoAcknowledgementTimeout NoAcknowledgementTimeout { get; }
	public Retries Retries { get; }
	public ManualAcknowledgementTimeout ManualAcknowledgementTimeout { get; }
	public TimeAndDate TimeAndDate { get; }
	public MdtTable MdtTable { get; }

	public static RouterParameterBootstrapConfiguration FromValues(
		CommunicationsAddress localAddress,
		NodeName nodeName,
		MaximumMessageLength maximumMessageLength,
		CommunicationsAddress networkManagerAddress1,
		CommunicationsAddress networkManagerAddress2,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries,
		ManualAcknowledgementTimeout manualAcknowledgementTimeout,
		TimeAndDate timeAndDate,
		MdtTable mdtTable)
	{
		ArgumentNullException.ThrowIfNull(localAddress);
		ArgumentNullException.ThrowIfNull(nodeName);
		ArgumentNullException.ThrowIfNull(maximumMessageLength);
		ArgumentNullException.ThrowIfNull(networkManagerAddress1);
		ArgumentNullException.ThrowIfNull(networkManagerAddress2);
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(retries);
		ArgumentNullException.ThrowIfNull(manualAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(timeAndDate);
		ArgumentNullException.ThrowIfNull(mdtTable);

		return new RouterParameterBootstrapConfiguration(
			localAddress,
			nodeName,
			maximumMessageLength,
			networkManagerAddress1,
			networkManagerAddress2,
			initialLevel1Password,
			noAcknowledgementTimeout,
			retries,
			manualAcknowledgementTimeout,
			timeAndDate,
			mdtTable);
	}
}
