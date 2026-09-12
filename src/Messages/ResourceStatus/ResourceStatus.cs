using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ResourceStatus : IGD92MessageContents
{
	private static readonly MessageType ResourceStatusMessageType =
		MessageType.FromValue(GD92MessageType.ResourceStatus);

	private ResourceStatus(IReadOnlyList<ResourceStatusEntry> statuses)
	{
		this.Statuses = statuses;
	}

	public IReadOnlyList<ResourceStatusEntry> Statuses { get; }
	public MessageType Type => ResourceStatusMessageType;

	public static ResourceStatus FromStatuses(params ResourceStatusEntry[] statuses)
	{
		ArgumentNullException.ThrowIfNull(statuses);

		if (statuses.Any(status => status is null))
		{
			throw new ArgumentException("Resource statuses may not contain null values.", nameof(statuses));
		}

		return new ResourceStatus(Array.AsReadOnly(statuses.ToArray()));
	}

	public static ResourceStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var statuses = new List<ResourceStatusEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			statuses.Add(ResourceStatusEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromStatuses(statuses.ToArray());
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte>();

		foreach (var status in this.Statuses)
		{
			wireValue.AddRange(status.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
