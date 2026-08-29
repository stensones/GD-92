namespace Stensones.GD92.Fields;

public sealed record AcknowledgementRequest
{
	private AcknowledgementRequest(bool isRequested)
	{
		this.IsRequested = isRequested;
	}

	public static AcknowledgementRequest Requested { get; } = new(true);
	public static AcknowledgementRequest NotRequested { get; } = new(false);

	public bool IsRequested { get; }
}
