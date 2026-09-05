using Stensones.GD92.Fields;
namespace Router.Persistence;

public sealed class RouterCurrentParameterProjection
{
	private static readonly PasswordLevel LevelZero =
		PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated);
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.Level1);
	private static readonly Password EmptyPassword = Password.FromValue(
		PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty)));
	private RouterCurrentParameterProjection(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier,
		PasswordParameter currentPassword,
		PasswordVerifier level1PasswordVerifier,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		this.BrigadeOrAgencyIdentifier = brigadeOrAgencyIdentifier;
		this.CurrentPassword = currentPassword;
		this.Level1PasswordVerifier = level1PasswordVerifier;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
	}

	public BrigadeOrAgencyIdentifier BrigadeOrAgencyIdentifier { get; }
	public PasswordParameter CurrentPassword { get; }
	public PasswordVerifier Level1PasswordVerifier { get; }
	public NoAcknowledgementTimeout NoAcknowledgementTimeout { get; }
	public Retries Retries { get; }

	public RouterCurrentParameterProjection LogOnAtLevelOne(
		CommunicationsAddress communicationsAddress)
	{
		ArgumentNullException.ThrowIfNull(communicationsAddress);

		return new RouterCurrentParameterProjection(
			this.BrigadeOrAgencyIdentifier,
			PasswordParameter.FromFields(LevelOne, EmptyPassword, communicationsAddress),
			this.Level1PasswordVerifier,
			this.NoAcknowledgementTimeout,
			this.Retries);
	}

	public RouterCurrentParameterProjection LogOffAtLevelZero(
		CommunicationsAddress localAddress)
	{
		ArgumentNullException.ThrowIfNull(localAddress);

		return new RouterCurrentParameterProjection(
			this.BrigadeOrAgencyIdentifier,
			PasswordParameter.FromFields(LevelZero, EmptyPassword, localAddress),
			this.Level1PasswordVerifier,
			this.NoAcknowledgementTimeout,
			this.Retries);
	}

	public RouterCurrentParameterProjection WithLevel1PasswordVerifier(
		PasswordVerifier level1PasswordVerifier)
	{
		ArgumentNullException.ThrowIfNull(level1PasswordVerifier);

		return new RouterCurrentParameterProjection(
			this.BrigadeOrAgencyIdentifier,
			this.CurrentPassword,
			level1PasswordVerifier,
			this.NoAcknowledgementTimeout,
			this.Retries);
	}

	public static RouterCurrentParameterProjection FromNonVolatileParameters(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier,
		PasswordParameter currentPassword,
		PasswordVerifier level1PasswordVerifier,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		ArgumentNullException.ThrowIfNull(currentPassword);
		ArgumentNullException.ThrowIfNull(level1PasswordVerifier);
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(retries);

		return new RouterCurrentParameterProjection(
			brigadeOrAgencyIdentifier,
			currentPassword,
			level1PasswordVerifier,
			noAcknowledgementTimeout,
			retries);
	}
}
