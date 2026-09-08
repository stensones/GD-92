using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record ManagementTransactionNoAcknowledgementTimeout
{
	private ManagementTransactionNoAcknowledgementTimeout(Word8 seconds)
	{
		this.Seconds = seconds;
	}

	public Word8 Seconds { get; }
	public TimeSpan Duration => TimeSpan.FromSeconds(this.Seconds.Value);

	public static ManagementTransactionNoAcknowledgementTimeout FromValue(Word8 seconds)
	{
		ArgumentNullException.ThrowIfNull(seconds);

		if (seconds.Value == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(seconds),
				"No acknowledgement timeout must be at least one second.");
		}

		return new ManagementTransactionNoAcknowledgementTimeout(seconds);
	}
}

public sealed record ManagementTransactionTotalSends
{
	private ManagementTransactionTotalSends(Word8 value)
	{
		this.Value = value;
	}

	public Word8 Value { get; }

	public static ManagementTransactionTotalSends FromValue(Word8 value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (value.Value == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(value),
				"Management Transaction total sends must be at least one.");
		}

		return new ManagementTransactionTotalSends(value);
	}
}

public sealed record ManagementTransactionRetryPolicy(
	ManagementTransactionNoAcknowledgementTimeout NoAcknowledgementTimeout,
	ManagementTransactionTotalSends TotalSends)
{
	public static ManagementTransactionRetryPolicy FromValues(
		ManagementTransactionNoAcknowledgementTimeout noAcknowledgementTimeout,
		ManagementTransactionTotalSends totalSends)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(totalSends);

		return new ManagementTransactionRetryPolicy(noAcknowledgementTimeout, totalSends);
	}
}
