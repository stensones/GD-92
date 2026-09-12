using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public static class RouterParameterCatalogue
{
	public static RouterParameterDefinition<BrigadeOrAgencyIdentifier> BrigadeOrAgency { get; } =
		RouterParameterDefinition<BrigadeOrAgencyIdentifier>.Create(
			ParameterNumber.FromValue(1),
			EncodeBrigadeOrAgencyIdentifier,
			ReadBrigadeOrAgencyIdentifier);

	public static RouterParameterDefinition<PasswordParameter> CurrentPassword { get; } =
		RouterParameterDefinition<PasswordParameter>.Create(
			ParameterNumber.FromValue(4),
			EncodeCurrentPassword,
			ReadCurrentPassword);

	public static RouterParameterDefinition<NoAcknowledgementTimeout> NoAcknowledgementTimeout { get; } =
		RouterParameterDefinition<NoAcknowledgementTimeout>.Create(
			ParameterNumber.FromValue(12),
			EncodeNoAcknowledgementTimeout,
			ReadNoAcknowledgementTimeout);

	public static RouterParameterDefinition<Retries> Retries { get; } =
		RouterParameterDefinition<Retries>.Create(
			ParameterNumber.FromValue(19),
			EncodeRetries,
			ReadRetries);

	public static ParameterNumber Level1PasswordNumber { get; } = ParameterNumber.FromValue(5);

	private static ParameterValue EncodeBrigadeOrAgencyIdentifier(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		return ParameterValue.FromWireValue(brigadeOrAgencyIdentifier.ToWireValue());
	}

	private static BrigadeOrAgencyIdentifier ReadBrigadeOrAgencyIdentifier(
		ParameterValue parameterValue)
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

	private static ParameterValue EncodeCurrentPassword(PasswordParameter currentPassword)
	{
		ArgumentNullException.ThrowIfNull(currentPassword);

		return ParameterValue.FromWireValue(currentPassword.ToWireValue());
	}

	private static PasswordParameter ReadCurrentPassword(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var currentPassword = PasswordParameter.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, CurrentPassword.Number);
		return currentPassword;
	}

	private static ParameterValue EncodeNoAcknowledgementTimeout(
		NoAcknowledgementTimeout noAcknowledgementTimeout)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);

		return ParameterValue.FromWireValue(noAcknowledgementTimeout.ToWireValue());
	}

	private static NoAcknowledgementTimeout ReadNoAcknowledgementTimeout(
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var noAcknowledgementTimeout = global::Stensones.GD92.Fields.NoAcknowledgementTimeout
			.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, NoAcknowledgementTimeout.Number);
		return noAcknowledgementTimeout;
	}

	private static ParameterValue EncodeRetries(Retries retries)
	{
		ArgumentNullException.ThrowIfNull(retries);

		return ParameterValue.FromWireValue(retries.ToWireValue());
	}

	private static Retries ReadRetries(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var retries = global::Stensones.GD92.Fields.Retries.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, Retries.Number);
		return retries;
	}

	private static void EnsureCompletelyRead(
		EncodedMessageBuffer buffer,
		ParameterValue parameterValue,
		ParameterNumber parameterNumber)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Router Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}
	}
}

public sealed class RouterParameterDefinition<T>
{
	private readonly Func<T, ParameterValue> encode;
	private readonly Func<ParameterValue, T> read;

	private RouterParameterDefinition(
		ParameterNumber number,
		Func<T, ParameterValue> encode,
		Func<ParameterValue, T> read)
	{
		this.Number = number;
		this.encode = encode;
		this.read = read;
	}

	public ParameterNumber Number { get; }

	public ParameterValue Encode(T value)
	{
		return this.encode(value);
	}

	public T Read(ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		return this.read(parameterValue);
	}

	public static RouterParameterDefinition<T> Create(
		ParameterNumber number,
		Func<T, ParameterValue> encode,
		Func<ParameterValue, T> read)
	{
		ArgumentNullException.ThrowIfNull(number);
		ArgumentNullException.ThrowIfNull(encode);
		ArgumentNullException.ThrowIfNull(read);

		return new RouterParameterDefinition<T>(number, encode, read);
	}
}
