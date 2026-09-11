using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ResourceStatusRequest : IGD92MessageContents
{
	private static readonly MessageType ResourceStatusRequestMessageType =
		MessageType.FromValue(GD92MessageType.ResourceStatusRequest);

	private ResourceStatusRequest(IReadOnlyList<Callsign> callsigns)
	{
		this.Callsigns = callsigns;
	}

	public IReadOnlyList<Callsign> Callsigns { get; }
	public MessageType Type => ResourceStatusRequestMessageType;

	public static ResourceStatusRequest FromCallsigns(params Callsign[] callsigns)
	{
		ArgumentNullException.ThrowIfNull(callsigns);

		if (callsigns.Length == 0)
		{
			throw new ArgumentException("At least one Callsign is required.", nameof(callsigns));
		}

		if (callsigns.Any(callsign => callsign is null))
		{
			throw new ArgumentException("Callsigns may not contain null values.", nameof(callsigns));
		}

		return new ResourceStatusRequest(Array.AsReadOnly(callsigns.ToArray()));
	}

	public static ResourceStatusRequest ForAllResources()
	{
		return FromCallsigns(Callsign.FromValue(SevenBitAsciiString.FromValue(string.Empty)));
	}

	public static ResourceStatusRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var callsigns = new List<Callsign>();

		while (buffer.RemainingBitCount > 0)
		{
			callsigns.Add(Callsign.FromEncodedMessageBuffer(ref buffer));
		}

		return FromCallsigns([.. callsigns]);
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte>();

		foreach (var callsign in this.Callsigns)
		{
			wireValue.AddRange(callsign.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
