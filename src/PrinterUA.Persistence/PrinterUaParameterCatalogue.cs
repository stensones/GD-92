using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace PrinterUA.Persistence;

internal static class PrinterUaParameterCatalogue
{
	internal static ParameterNumber PortNumber { get; } = ParameterNumber.FromValue(1);
	internal static ParameterNumber AgentType { get; } = ParameterNumber.FromValue(2);
	internal static ParameterNumber ControlAddress { get; } = ParameterNumber.FromValue(3);

	internal static PrinterUaParameterDefinition<AddressRange> DefaultSource { get; } =
		PrinterUaParameterDefinition<AddressRange>.Create(
			ParameterNumber.FromValue(21),
			EncodeAddressRange,
			ReadDefaultSource);

	internal static PrinterUaParameterDefinition<ProtocolBoolean> NotifyPrinterAvailable { get; } =
		PrinterUaParameterDefinition<ProtocolBoolean>.Create(
			ParameterNumber.FromValue(22),
			EncodeProtocolBoolean,
			ReadNotifyPrinterAvailable);

	internal static PrinterUaParameterDefinition<AddressTable> AlternativeSource { get; } =
		PrinterUaParameterDefinition<AddressTable>.Create(
			ParameterNumber.FromValue(23),
			EncodeAddressTable,
			ReadAlternativeSource);

	internal static PrinterUaParameterDefinition<ProtocolBoolean> ReprintMessage { get; } =
		PrinterUaParameterDefinition<ProtocolBoolean>.Create(
			ParameterNumber.FromValue(24),
			EncodeProtocolBoolean,
			ReadReprintMessage);

	private static ParameterValue EncodeAddressRange(AddressRange addressRange)
	{
		ArgumentNullException.ThrowIfNull(addressRange);

		return ParameterValue.FromWireValue(addressRange.ToWireValue());
	}

	private static AddressRange ReadDefaultSource(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var addressRange = AddressRange.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, DefaultSource.Number);
		return addressRange;
	}

	private static ParameterValue EncodeProtocolBoolean(ProtocolBoolean protocolBoolean)
	{
		return ParameterValue.FromWireValue(protocolBoolean.ToWireValue());
	}

	private static ProtocolBoolean ReadNotifyPrinterAvailable(ParameterValue parameterValue)
	{
		return ReadProtocolBoolean(parameterValue, NotifyPrinterAvailable.Number);
	}

	private static ProtocolBoolean ReadReprintMessage(ParameterValue parameterValue)
	{
		return ReadProtocolBoolean(parameterValue, ReprintMessage.Number);
	}

	private static ProtocolBoolean ReadProtocolBoolean(
		ParameterValue parameterValue,
		ParameterNumber parameterNumber)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var protocolBoolean = ProtocolBoolean.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, parameterNumber);
		return protocolBoolean;
	}

	private static ParameterValue EncodeAddressTable(AddressTable addressTable)
	{
		ArgumentNullException.ThrowIfNull(addressTable);

		return ParameterValue.FromWireValue(addressTable.ToWireValue());
	}

	private static AddressTable ReadAlternativeSource(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var addressTable = AddressTable.FromEncodedMessageBuffer(ref buffer);

		EnsureCompletelyRead(buffer, parameterValue, AlternativeSource.Number);
		return addressTable;
	}

	private static void EnsureCompletelyRead(
		EncodedMessageBuffer buffer,
		ParameterValue parameterValue,
		ParameterNumber parameterNumber)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Printer UA Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}
	}
}

internal sealed class PrinterUaParameterDefinition<T>
{
	private readonly Func<T, ParameterValue> encode;
	private readonly Func<ParameterValue, T> read;

	private PrinterUaParameterDefinition(
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

	public static PrinterUaParameterDefinition<T> Create(
		ParameterNumber number,
		Func<T, ParameterValue> encode,
		Func<ParameterValue, T> read)
	{
		ArgumentNullException.ThrowIfNull(number);
		ArgumentNullException.ThrowIfNull(encode);
		ArgumentNullException.ThrowIfNull(read);

		return new PrinterUaParameterDefinition<T>(number, encode, read);
	}
}
