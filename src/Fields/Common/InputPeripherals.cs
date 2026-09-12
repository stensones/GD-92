namespace Stensones.GD92.Fields;

public sealed record InputPeripherals : IGD9Field
{
	private const ushort DefinedInputMask = 0x003F;
	private const ushort EmptyPeripheralMask = 0;
	private const int PeripheralBitMapBitCount = 16;
	private const int ByteBitCount = 8;

	private readonly ushort value;

	private InputPeripherals(ushort value)
	{
		this.value = value;
	}

	public ushort ReservedBits => (ushort)(this.value & ~DefinedInputMask);

	public static InputPeripherals FromInputs(params InputPeripheral[] inputs)
	{
		ArgumentNullException.ThrowIfNull(inputs);

		var value = EmptyPeripheralMask;

		foreach (var input in inputs)
		{
			if (!Enum.IsDefined(input))
			{
				throw new ArgumentOutOfRangeException(nameof(inputs));
			}

			value |= (ushort)input;
		}

		return new InputPeripherals(value);
	}

	public static InputPeripherals FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new InputPeripherals((ushort)buffer.ReadUnsignedBits(PeripheralBitMapBitCount));
	}

	public bool IsInputSet(InputPeripheral input)
	{
		if (!Enum.IsDefined(input))
		{
			throw new ArgumentOutOfRangeException(nameof(input));
		}

		return (this.value & (ushort)input) != 0;
	}

	public byte[] ToWireValue()
	{
		return [(byte)(this.value >> ByteBitCount), (byte)this.value];
	}
}
