using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA.Persistence;

public static class LanMtaParameterCatalogue
{
	internal static LanMtaParameterDefinition<Word8> PortNumber { get; } =
		LanMtaParameterDefinition<Word8>.Create(
			ParameterNumber.FromValue(1),
			static value => value.ToWireValue(),
			Word8.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<AgentType> AgentType { get; } =
		LanMtaParameterDefinition<AgentType>.Create(
			ParameterNumber.FromValue(2),
			static value => value.ToWireValue(),
			global::Stensones.GD92.Fields.AgentType.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<MtaStatus> InterfaceStatus { get; } =
		LanMtaParameterDefinition<MtaStatus>.Create(
			ParameterNumber.FromValue(3),
			static value => value.ToWireValue(),
			MtaStatus.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<ProtocolBoolean> NotifyStatusChanges { get; } =
		LanMtaParameterDefinition<ProtocolBoolean>.Create(
			ParameterNumber.FromValue(4),
			static value => value.ToWireValue(),
			ProtocolBoolean.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<FrameTransmitCount> FrameTransmitCount { get; } =
		LanMtaParameterDefinition<FrameTransmitCount>.Create(
			ParameterNumber.FromValue(5),
			static value => value.ToWireValue(),
			global::Stensones.GD92.Fields.FrameTransmitCount.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<FrameReceiveCount> FrameReceiveCount { get; } =
		LanMtaParameterDefinition<FrameReceiveCount>.Create(
			ParameterNumber.FromValue(6),
			static value => value.ToWireValue(),
			global::Stensones.GD92.Fields.FrameReceiveCount.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<FrameTransmitFailureCount>
		FrameTransmitFailureCount { get; } =
			LanMtaParameterDefinition<FrameTransmitFailureCount>.Create(
				ParameterNumber.FromValue(7),
				static value => value.ToWireValue(),
				global::Stensones.GD92.Fields.FrameTransmitFailureCount
					.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<FrameReceiveFailureCount>
		FrameReceiveFailureCount { get; } =
			LanMtaParameterDefinition<FrameReceiveFailureCount>.Create(
				ParameterNumber.FromValue(8),
				static value => value.ToWireValue(),
				global::Stensones.GD92.Fields.FrameReceiveFailureCount
					.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<MtaMinimumMessagePriority> Priority { get; } =
		LanMtaParameterDefinition<MtaMinimumMessagePriority>.Create(
			ParameterNumber.FromValue(9),
			static value => value.ToWireValue(),
			MtaMinimumMessagePriority.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<DestinationNodes> NextNodes { get; } =
		LanMtaParameterDefinition<DestinationNodes>.Create(
			ParameterNumber.FromValue(10),
			static value => value.ToWireValue(),
			DestinationNodes.FromEncodedMessageBuffer);

	internal static LanMtaParameterDefinition<LanAddress> MyLanAddress { get; } =
		LanMtaParameterDefinition<LanAddress>.Create(
			ParameterNumber.FromValue(21),
			static value => value.ToWireValue(),
			LanAddress.FromEncodedMessageBuffer);

	internal static MtaStatus IdleInterfaceStatus { get; } =
		MtaStatus.FromValue(MtaStatusValue.Idle);
	internal static ProtocolBoolean NotifyStatusChangesDefault { get; } = ProtocolBoolean.True;
	internal static FrameTransmitCount InitialFrameTransmitCount { get; } =
		global::Stensones.GD92.Fields.FrameTransmitCount.FromValue(0);
	internal static FrameReceiveCount InitialFrameReceiveCount { get; } =
		global::Stensones.GD92.Fields.FrameReceiveCount.FromValue(0);
	internal static FrameTransmitFailureCount InitialFrameTransmitFailureCount { get; } =
		global::Stensones.GD92.Fields.FrameTransmitFailureCount.FromValue(0);
	internal static FrameReceiveFailureCount InitialFrameReceiveFailureCount { get; } =
		global::Stensones.GD92.Fields.FrameReceiveFailureCount.FromValue(0);
	internal static MtaMinimumMessagePriority StandardPriority { get; } =
		MtaMinimumMessagePriority.FromValue(MessagePriorityLevel.FromValue(3));
	internal static DestinationNodes EmptyNextNodes { get; } = DestinationNodes.FromAddressRanges();
	internal static LanAddress StationEndLanAddress { get; } =
		LanAddress.FromValue(SevenBitAsciiString.FromValue("station-end-lan"));
}

internal delegate T LanMtaParameterFieldDecoder<T>(ref EncodedMessageBuffer buffer);

internal sealed class LanMtaParameterDefinition<T>
	where T : IGD9Field
{
	private readonly LanMtaParameterFieldDecoder<T> decode;
	private readonly Func<T, byte[]> encode;

	private LanMtaParameterDefinition(
		ParameterNumber number,
		Func<T, byte[]> encode,
		LanMtaParameterFieldDecoder<T> decode)
	{
		this.Number = number;
		this.encode = encode;
		this.decode = decode;
	}

	public ParameterNumber Number { get; }

	public ParameterValue Encode(T value)
	{
		return ParameterValue.FromWireValue(this.encode(value));
	}

	public T Decode(ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = this.decode(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"LAN MTA Parameter {this.Number.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}

		return value;
	}

	public static LanMtaParameterDefinition<T> Create(
		ParameterNumber number,
		Func<T, byte[]> encode,
		LanMtaParameterFieldDecoder<T> decode)
	{
		ArgumentNullException.ThrowIfNull(number);
		ArgumentNullException.ThrowIfNull(encode);
		ArgumentNullException.ThrowIfNull(decode);

		return new LanMtaParameterDefinition<T>(number, encode, decode);
	}
}
