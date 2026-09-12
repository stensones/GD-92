using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record RouteStatus : IGD92MessageContents
{
	private static readonly MessageType RouteStatusMessageType =
		MessageType.FromValue(GD92MessageType.RouteStatus);

	private RouteStatus(ProtocolBoolean routesEnabled, DestinationNodes destinationNodes)
	{
		this.RoutesEnabled = routesEnabled;
		this.DestinationNodes = destinationNodes;
	}

	public ProtocolBoolean RoutesEnabled { get; }
	public DestinationNodes DestinationNodes { get; }
	public MessageType Type => RouteStatusMessageType;

	public static RouteStatus FromFields(ProtocolBoolean routesEnabled, DestinationNodes destinationNodes)
	{
		ArgumentNullException.ThrowIfNull(destinationNodes);

		return new RouteStatus(routesEnabled, destinationNodes);
	}

	public static RouteStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			ProtocolBoolean.FromEncodedMessageBuffer(ref buffer),
			DestinationNodes.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.RoutesEnabled.ToWireValue(),
			.. this.DestinationNodes.ToWireValue()
		];
	}
}
