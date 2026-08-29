using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class NegativeAcknowledgement : IGD92MessageContents
{
	private static readonly MessageType NegativeAcknowledgementMessageType =
		MessageType.FromValue(GD92MessageType.NegativeAcknowledgement);

	private NegativeAcknowledgement(Destinations destinations, ReasonCode reasonCode)
	{
		this.Destinations = destinations;
		this.ReasonCode = reasonCode;
	}

	public Destinations Destinations { get; }
	public ReasonCode ReasonCode { get; }
	public MessageType Type => NegativeAcknowledgementMessageType;

	public static NegativeAcknowledgement FromValues(Destinations destinations, ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(destinations);
		ArgumentNullException.ThrowIfNull(reasonCode);

		return new NegativeAcknowledgement(destinations, reasonCode);
	}

	public byte[] ToWireValue()
	{
		return [this.Destinations.Count.Value, .. this.Destinations.ToWireValue(), .. this.ReasonCode.ToWireValue()];
	}
}
