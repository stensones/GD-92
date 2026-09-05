using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record NodeLoginNoAcknowledgementTimeout
{
	private NodeLoginNoAcknowledgementTimeout(Word8 seconds)
	{
		this.Seconds = seconds;
	}

	public Word8 Seconds { get; }
	public TimeSpan Duration => TimeSpan.FromSeconds(this.Seconds.Value);

	public static NodeLoginNoAcknowledgementTimeout FromValue(Word8 seconds)
	{
		ArgumentNullException.ThrowIfNull(seconds);

		if (seconds.Value == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(seconds),
				"No acknowledgement timeout must be at least one second.");
		}

		return new NodeLoginNoAcknowledgementTimeout(seconds);
	}
}

public sealed record NodeLoginTotalSends
{
	private NodeLoginTotalSends(Word8 value)
	{
		this.Value = value;
	}

	public Word8 Value { get; }

	public static NodeLoginTotalSends FromValue(Word8 value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (value.Value == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(value),
				"Node Login total sends must be at least one.");
		}

		return new NodeLoginTotalSends(value);
	}
}

public sealed record NodeLoginRetryPolicy(
	NodeLoginNoAcknowledgementTimeout NoAcknowledgementTimeout,
	NodeLoginTotalSends TotalSends)
{
	public static NodeLoginRetryPolicy FromValues(
		NodeLoginNoAcknowledgementTimeout noAcknowledgementTimeout,
		NodeLoginTotalSends totalSends)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(totalSends);

		return new NodeLoginRetryPolicy(noAcknowledgementTimeout, totalSends);
	}
}
