using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record PageOfficer : IGD92MessageContents
{
	private static readonly MessageType PageOfficerMessageType =
		MessageType.FromValue(GD92MessageType.PageOfficer);

	private PageOfficer(
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
	public MessageType Type => PageOfficerMessageType;

	public static PageOfficer FromFields(
		PagerPriority pagerPriority,
		PagerNumber pagerNumber,
		PagerText pagerText)
	{
		ArgumentNullException.ThrowIfNull(pagerPriority);
		ArgumentNullException.ThrowIfNull(pagerNumber);
		ArgumentNullException.ThrowIfNull(pagerText);

		return new PageOfficer(pagerPriority, pagerNumber, pagerText);
	}

	public static PageOfficer FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
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
