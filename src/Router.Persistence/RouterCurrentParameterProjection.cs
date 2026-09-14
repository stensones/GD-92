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
		PasswordVerifier level2PasswordVerifier,
		PasswordVerifier level3PasswordVerifier,
		PasswordVerifier level4PasswordVerifier,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		this.BrigadeOrAgencyIdentifier = brigadeOrAgencyIdentifier;
		this.CurrentPassword = currentPassword;
		this.Level1PasswordVerifier = level1PasswordVerifier;
		this.Level2PasswordVerifier = level2PasswordVerifier;
		this.Level3PasswordVerifier = level3PasswordVerifier;
		this.Level4PasswordVerifier = level4PasswordVerifier;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
	}

	public BrigadeOrAgencyIdentifier BrigadeOrAgencyIdentifier { get; }
	public PasswordParameter CurrentPassword { get; }
	public PasswordVerifier Level1PasswordVerifier { get; }
	public PasswordVerifier Level2PasswordVerifier { get; }
	public PasswordVerifier Level3PasswordVerifier { get; }
	public PasswordVerifier Level4PasswordVerifier { get; }
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
			this.Level2PasswordVerifier,
			this.Level3PasswordVerifier,
			this.Level4PasswordVerifier,
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
			this.Level2PasswordVerifier,
			this.Level3PasswordVerifier,
			this.Level4PasswordVerifier,
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
			this.Level2PasswordVerifier,
			this.Level3PasswordVerifier,
			this.Level4PasswordVerifier,
			this.NoAcknowledgementTimeout,
			this.Retries);
	}

	public RouterCurrentParameterProjection WithRetries(Retries retries)
	{
		ArgumentNullException.ThrowIfNull(retries);

		return new RouterCurrentParameterProjection(
			this.BrigadeOrAgencyIdentifier,
			this.CurrentPassword,
			this.Level1PasswordVerifier,
			this.Level2PasswordVerifier,
			this.Level3PasswordVerifier,
			this.Level4PasswordVerifier,
			this.NoAcknowledgementTimeout,
			retries);
	}

	public RouterCurrentParameterProjection WithNoAcknowledgementTimeout(
		NoAcknowledgementTimeout noAcknowledgementTimeout)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);

		return new RouterCurrentParameterProjection(
			this.BrigadeOrAgencyIdentifier,
			this.CurrentPassword,
			this.Level1PasswordVerifier,
			this.Level2PasswordVerifier,
			this.Level3PasswordVerifier,
			this.Level4PasswordVerifier,
			noAcknowledgementTimeout,
			this.Retries);
	}

	public static RouterCurrentParameterProjection FromNonVolatileParameters(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier,
		PasswordParameter currentPassword,
		PasswordVerifier level1PasswordVerifier,
		PasswordVerifier level2PasswordVerifier,
		PasswordVerifier level3PasswordVerifier,
		PasswordVerifier level4PasswordVerifier,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		ArgumentNullException.ThrowIfNull(currentPassword);
		ArgumentNullException.ThrowIfNull(level1PasswordVerifier);
		ArgumentNullException.ThrowIfNull(level2PasswordVerifier);
		ArgumentNullException.ThrowIfNull(level3PasswordVerifier);
		ArgumentNullException.ThrowIfNull(level4PasswordVerifier);
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(retries);

		return new RouterCurrentParameterProjection(
			brigadeOrAgencyIdentifier,
			currentPassword,
			level1PasswordVerifier,
			level2PasswordVerifier,
			level3PasswordVerifier,
			level4PasswordVerifier,
			noAcknowledgementTimeout,
			retries);
	}
}
