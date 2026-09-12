using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public static class RouterParameterCatalogue
{
	public static RouterParameterDefinition<BrigadeOrAgencyIdentifier> BrigadeOrAgency { get; } =
		RouterParameterDefinition<BrigadeOrAgencyIdentifier>.Create(
			ParameterNumber.FromValue(1),
			EncodeBrigadeOrAgencyIdentifier,
			ReadBrigadeOrAgencyIdentifier);

	public static RouterParameterDefinition<ushort> NodeNumber { get; } =
		RouterParameterDefinition<ushort>.Create(
			ParameterNumber.FromValue(2),
			EncodeNodeNumber,
			ReadNodeNumber);

	public static RouterParameterDefinition<NodeName> NodeName { get; } =
		RouterParameterDefinition<NodeName>.Create(
			ParameterNumber.FromValue(3),
			EncodeNodeName,
			ReadNodeName);

	public static RouterParameterDefinition<PasswordParameter> CurrentPassword { get; } =
		RouterParameterDefinition<PasswordParameter>.Create(
			ParameterNumber.FromValue(4),
			EncodeCurrentPassword,
			ReadCurrentPassword);

	public static RouterParameterDefinition<MaximumMessageLength> MaximumMessageLength { get; } =
		RouterParameterDefinition<MaximumMessageLength>.Create(
			ParameterNumber.FromValue(9),
			EncodeMaximumMessageLength,
			ReadMaximumMessageLength);

	public static RouterParameterDefinition<CommunicationsAddress> NetworkManagerAddress1 { get; } =
		RouterParameterDefinition<CommunicationsAddress>.Create(
			ParameterNumber.FromValue(10),
			EncodeCommunicationsAddress,
			ReadNetworkManagerAddress1);

	public static RouterParameterDefinition<CommunicationsAddress> NetworkManagerAddress2 { get; } =
		RouterParameterDefinition<CommunicationsAddress>.Create(
			ParameterNumber.FromValue(11),
			EncodeCommunicationsAddress,
			ReadNetworkManagerAddress2);

	public static RouterParameterDefinition<NoAcknowledgementTimeout> NoAcknowledgementTimeout { get; } =
		RouterParameterDefinition<NoAcknowledgementTimeout>.Create(
			ParameterNumber.FromValue(12),
			EncodeNoAcknowledgementTimeout,
			ReadNoAcknowledgementTimeout);

	public static RouterParameterDefinition<RoutingTable> RouterTable { get; } =
		RouterParameterDefinition<RoutingTable>.Create(
			ParameterNumber.FromValue(13),
			EncodeRoutingTable,
			ReadRoutingTable);

	public static RouterParameterDefinition<PstnTable> PstnTable { get; } =
		RouterParameterDefinition<PstnTable>.Create(
			ParameterNumber.FromValue(14),
			EncodePstnTable,
			ReadPstnTable);

	public static RouterParameterDefinition<WanTable> WanTable { get; } =
		RouterParameterDefinition<WanTable>.Create(
			ParameterNumber.FromValue(15),
			EncodeWanTable,
			ReadWanTable);

	public static RouterParameterDefinition<LanTable> LanTable { get; } =
		RouterParameterDefinition<LanTable>.Create(
			ParameterNumber.FromValue(16),
			EncodeLanTable,
			ReadLanTable);

	public static RouterParameterDefinition<IsdnTable> IsdnTable { get; } =
		RouterParameterDefinition<IsdnTable>.Create(
			ParameterNumber.FromValue(17),
			EncodeIsdnTable,
			ReadIsdnTable);

	public static RouterParameterDefinition<ManualAcknowledgementTimeout>
		ManualAcknowledgementTimeout { get; } =
		RouterParameterDefinition<ManualAcknowledgementTimeout>.Create(
			ParameterNumber.FromValue(18),
			EncodeManualAcknowledgementTimeout,
			ReadManualAcknowledgementTimeout);

	public static RouterParameterDefinition<Retries> Retries { get; } =
		RouterParameterDefinition<Retries>.Create(
			ParameterNumber.FromValue(19),
			EncodeRetries,
			ReadRetries);

	public static RouterParameterDefinition<TimeAndDate> TimeAndDate { get; } =
		RouterParameterDefinition<TimeAndDate>.Create(
			ParameterNumber.FromValue(20),
			EncodeTimeAndDate,
			ReadTimeAndDate);

	public static RouterParameterDefinition<MdtTable> MdtTable { get; } =
		RouterParameterDefinition<MdtTable>.Create(
			ParameterNumber.FromValue(21),
			EncodeMdtTable,
			ReadMdtTable);

	public static ParameterNumber Level1PasswordNumber { get; } = ParameterNumber.FromValue(5);

	private static ParameterValue EncodeBrigadeOrAgencyIdentifier(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		return ParameterValue.FromWireValue(brigadeOrAgencyIdentifier.ToWireValue());
	}

	private static BrigadeOrAgencyIdentifier ReadBrigadeOrAgencyIdentifier(
		ParameterValue parameterValue)
	{
		var encodedValue = parameterValue.ToWireValue();
		if (encodedValue.Length != 1)
		{
			throw new ArgumentException(
				"Router Parameter 1 must contain exactly one encoded octet.",
				nameof(parameterValue));
		}

		return BrigadeOrAgencyIdentifier.FromValue(encodedValue[0]);
	}

	private static ParameterValue EncodeNodeNumber(ushort nodeNumber)
	{
		return ParameterValue.FromWireValue([(byte)(nodeNumber >> 8), (byte)nodeNumber]);
	}

	private static ushort ReadNodeNumber(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var nodeNumber = (ushort)buffer.ReadUnsignedBits(16);

		EnsureCompletelyRead(buffer, parameterValue, NodeNumber.Number);
		return nodeNumber;
	}

	private static ParameterValue EncodeNodeName(NodeName nodeName)
	{
		ArgumentNullException.ThrowIfNull(nodeName);

		return ParameterValue.FromWireValue(nodeName.ToWireValue());
	}

	private static NodeName ReadNodeName(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var nodeName = global::Stensones.GD92.Fields.NodeName.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, NodeName.Number);
		return nodeName;
	}

	private static ParameterValue EncodeCurrentPassword(PasswordParameter currentPassword)
	{
		ArgumentNullException.ThrowIfNull(currentPassword);

		return ParameterValue.FromWireValue(currentPassword.ToWireValue());
	}

	private static PasswordParameter ReadCurrentPassword(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var currentPassword = PasswordParameter.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, CurrentPassword.Number);
		return currentPassword;
	}

	private static ParameterValue EncodeMaximumMessageLength(MaximumMessageLength maximumMessageLength)
	{
		ArgumentNullException.ThrowIfNull(maximumMessageLength);

		return ParameterValue.FromWireValue(maximumMessageLength.ToWireValue());
	}

	private static MaximumMessageLength ReadMaximumMessageLength(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var maximumMessageLength = global::Stensones.GD92.Fields.MaximumMessageLength
			.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, MaximumMessageLength.Number);
		return maximumMessageLength;
	}

	private static ParameterValue EncodeCommunicationsAddress(
		CommunicationsAddress communicationsAddress)
	{
		ArgumentNullException.ThrowIfNull(communicationsAddress);

		return ParameterValue.FromWireValue(communicationsAddress.ToWireValue());
	}

	private static CommunicationsAddress ReadNetworkManagerAddress1(ParameterValue parameterValue)
	{
		return ReadCommunicationsAddress(parameterValue, NetworkManagerAddress1.Number);
	}

	private static CommunicationsAddress ReadNetworkManagerAddress2(ParameterValue parameterValue)
	{
		return ReadCommunicationsAddress(parameterValue, NetworkManagerAddress2.Number);
	}

	private static CommunicationsAddress ReadCommunicationsAddress(
		ParameterValue parameterValue,
		ParameterNumber parameterNumber)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var communicationsAddress = CommunicationsAddress.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, parameterNumber);
		return communicationsAddress;
	}

	private static ParameterValue EncodeNoAcknowledgementTimeout(
		NoAcknowledgementTimeout noAcknowledgementTimeout)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);

		return ParameterValue.FromWireValue(noAcknowledgementTimeout.ToWireValue());
	}

	private static NoAcknowledgementTimeout ReadNoAcknowledgementTimeout(
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var noAcknowledgementTimeout = global::Stensones.GD92.Fields.NoAcknowledgementTimeout
			.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, NoAcknowledgementTimeout.Number);
		return noAcknowledgementTimeout;
	}

	private static ParameterValue EncodeRoutingTable(RoutingTable routingTable)
	{
		ArgumentNullException.ThrowIfNull(routingTable);

		return ParameterValue.FromWireValue(routingTable.ToWireValue());
	}

	private static RoutingTable ReadRoutingTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var routingTable = RoutingTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, RouterTable.Number);
		return routingTable;
	}

	private static ParameterValue EncodePstnTable(PstnTable pstnTable)
	{
		ArgumentNullException.ThrowIfNull(pstnTable);

		return ParameterValue.FromWireValue(pstnTable.ToWireValue());
	}

	private static PstnTable ReadPstnTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var pstnTable = global::Stensones.GD92.Fields.PstnTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, PstnTable.Number);
		return pstnTable;
	}

	private static ParameterValue EncodeWanTable(WanTable wanTable)
	{
		ArgumentNullException.ThrowIfNull(wanTable);

		return ParameterValue.FromWireValue(wanTable.ToWireValue());
	}

	private static WanTable ReadWanTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var wanTable = global::Stensones.GD92.Fields.WanTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, WanTable.Number);
		return wanTable;
	}

	private static ParameterValue EncodeLanTable(LanTable lanTable)
	{
		ArgumentNullException.ThrowIfNull(lanTable);

		return ParameterValue.FromWireValue(lanTable.ToWireValue());
	}

	private static LanTable ReadLanTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var lanTable = global::Stensones.GD92.Fields.LanTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, LanTable.Number);
		return lanTable;
	}

	private static ParameterValue EncodeIsdnTable(IsdnTable isdnTable)
	{
		ArgumentNullException.ThrowIfNull(isdnTable);

		return ParameterValue.FromWireValue(isdnTable.ToWireValue());
	}

	private static IsdnTable ReadIsdnTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var isdnTable = global::Stensones.GD92.Fields.IsdnTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, IsdnTable.Number);
		return isdnTable;
	}

	private static ParameterValue EncodeRetries(Retries retries)
	{
		ArgumentNullException.ThrowIfNull(retries);

		return ParameterValue.FromWireValue(retries.ToWireValue());
	}

	private static Retries ReadRetries(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var retries = global::Stensones.GD92.Fields.Retries.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, Retries.Number);
		return retries;
	}

	private static ParameterValue EncodeManualAcknowledgementTimeout(
		ManualAcknowledgementTimeout manualAcknowledgementTimeout)
	{
		ArgumentNullException.ThrowIfNull(manualAcknowledgementTimeout);

		return ParameterValue.FromWireValue(manualAcknowledgementTimeout.ToWireValue());
	}

	private static ManualAcknowledgementTimeout ReadManualAcknowledgementTimeout(
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var manualAcknowledgementTimeout =
			global::Stensones.GD92.Fields.ManualAcknowledgementTimeout
				.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, ManualAcknowledgementTimeout.Number);
		return manualAcknowledgementTimeout;
	}

	private static ParameterValue EncodeTimeAndDate(TimeAndDate timeAndDate)
	{
		ArgumentNullException.ThrowIfNull(timeAndDate);

		return ParameterValue.FromWireValue(timeAndDate.ToWireValue());
	}

	private static TimeAndDate ReadTimeAndDate(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var timeAndDate = global::Stensones.GD92.Fields.TimeAndDate
			.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, TimeAndDate.Number);
		return timeAndDate;
	}

	private static ParameterValue EncodeMdtTable(MdtTable mdtTable)
	{
		ArgumentNullException.ThrowIfNull(mdtTable);

		return ParameterValue.FromWireValue(mdtTable.ToWireValue());
	}

	private static MdtTable ReadMdtTable(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var mdtTable = global::Stensones.GD92.Fields.MdtTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, MdtTable.Number);
		return mdtTable;
	}

	private static void EnsureCompletelyRead(
		EncodedMessageBuffer buffer,
		ParameterValue parameterValue,
		ParameterNumber parameterNumber)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Router Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}
	}
}

public sealed class RouterParameterDefinition<T>
{
	private readonly Func<T, ParameterValue> encode;
	private readonly Func<ParameterValue, T> read;

	private RouterParameterDefinition(
		ParameterNumber number,
		Func<T, ParameterValue> encode,
		Func<ParameterValue, T> read)
	{
		this.Number = number;
		this.encode = encode;
		this.read = read;
	}

	public ParameterNumber Number { get; }

	public ParameterValue Encode(T value)
	{
		return this.encode(value);
	}

	public T Read(ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		return this.read(parameterValue);
	}

	public static RouterParameterDefinition<T> Create(
		ParameterNumber number,
		Func<T, ParameterValue> encode,
		Func<ParameterValue, T> read)
	{
		ArgumentNullException.ThrowIfNull(number);
		ArgumentNullException.ThrowIfNull(encode);
		ArgumentNullException.ThrowIfNull(read);

		return new RouterParameterDefinition<T>(number, encode, read);
	}
}
