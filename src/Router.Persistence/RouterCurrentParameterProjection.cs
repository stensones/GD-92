using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

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

	public static RouterCurrentParameterProjection FromNonVolatileValues(
		ParameterValue brigadeOrAgencyIdentifier,
		ParameterValue currentPassword,
		PasswordVerifier level1PasswordVerifier,
		ParameterValue noAcknowledgementTimeout,
		ParameterValue retries)
	{
		ArgumentNullException.ThrowIfNull(brigadeOrAgencyIdentifier);
		ArgumentNullException.ThrowIfNull(currentPassword);
		ArgumentNullException.ThrowIfNull(level1PasswordVerifier);
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(retries);

		return new RouterCurrentParameterProjection(
			ReadParameterOne(brigadeOrAgencyIdentifier),
			ReadCurrentPassword(currentPassword),
			level1PasswordVerifier,
			ReadNoAcknowledgementTimeout(noAcknowledgementTimeout),
			ReadRetries(retries));
	}

	private static BrigadeOrAgencyIdentifier ReadParameterOne(ParameterValue parameterValue)
	{
		var encodedValue = parameterValue.ToWireValue();
		if (encodedValue.Length != 1)
		{
			throw new ArgumentException(
				"Router Parameter 1 must contain exactly one encoded octet.",
				nameof(parameterValue));
		}

		return BrigadeOrAgencyIdentifier.FromValue(encodedValue[0]);
	}

	private static PasswordParameter ReadCurrentPassword(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = PasswordParameter.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, 4);
		return value;
	}

	private static NoAcknowledgementTimeout ReadNoAcknowledgementTimeout(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = NoAcknowledgementTimeout.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, 12);
		return value;
	}

	private static Retries ReadRetries(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = Retries.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, 19);
		return value;
	}

	private static void EnsureCompletelyRead(
		EncodedMessageBuffer buffer,
		ParameterValue parameterValue,
		byte parameterNumber)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Router Parameter {parameterNumber} contains trailing encoded data.",
				nameof(parameterValue));
		}
	}
}
