using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record AreaPageMessage : IGD92MessageContents
{
	private static readonly MessageType AreaPageMessageType =
		MessageType.FromValue(GD92MessageType.AreaPageMessage);

	private AreaPageMessage(
		PagerPriority pagerPriority,
		PagerNumber pagerNumber,
		PagerText pagerText)
	{
		this.PagerPriority = pagerPriority;
		this.PagerNumber = pagerNumber;
		this.PagerText = pagerText;
	}

	public PagerPriority PagerPriority { get; }
	public PagerNumber PagerNumber { get; }
	public PagerText PagerText { get; }
	public MessageType Type => AreaPageMessageType;

	public static AreaPageMessage FromFields(
		PagerPriority pagerPriority,
		PagerNumber pagerNumber,
		PagerText pagerText)
	{
		ArgumentNullException.ThrowIfNull(pagerPriority);
		ArgumentNullException.ThrowIfNull(pagerNumber);
		ArgumentNullException.ThrowIfNull(pagerText);

		return new AreaPageMessage(pagerPriority, pagerNumber, pagerText);
	}

	public static AreaPageMessage FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			PagerPriority.FromEncodedMessageBuffer(ref buffer),
			PagerNumber.FromEncodedMessageBuffer(ref buffer),
			PagerText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.PagerPriority.ToWireValue(),
			.. this.PagerNumber.ToWireValue(),
			.. this.PagerText.ToWireValue()
		];
	}
}
