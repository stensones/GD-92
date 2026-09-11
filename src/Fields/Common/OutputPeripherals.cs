namespace Stensones.GD92.Fields;

public sealed record OutputPeripherals : IGD9Field
{
	private const ushort DefinedOutputMask = 0xFF63;
	private const int PeripheralBitMapBitCount = 16;
	private const int ByteBitCount = 8;

	private readonly ushort value;

	private OutputPeripherals(ushort value)
	{
		this.value = value;
	}

	public ushort ReservedBits => (ushort)(this.value & ~DefinedOutputMask);

	public static OutputPeripherals FromOutputs(params OutputPeripheral[] outputs)
	{
		ArgumentNullException.ThrowIfNull(outputs);

		var value = (ushort)0;

		foreach (var output in outputs)
		{
			if (!Enum.IsDefined(output))
			{
				throw new ArgumentOutOfRangeException(nameof(outputs));
			}

			value |= (ushort)output;
		}

		return new OutputPeripherals(value);
	}

	public static OutputPeripherals FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new OutputPeripherals((ushort)buffer.ReadUnsignedBits(PeripheralBitMapBitCount));
	}

	public bool IsOutputSet(OutputPeripheral output)
	{
		if (!Enum.IsDefined(output))
		{
			throw new ArgumentOutOfRangeException(nameof(output));
		}

		return (this.value & (ushort)output) != 0;
	}

	public byte[] ToWireValue()
	{
		return [(byte)(this.value >> ByteBitCount), (byte)this.value];
	}
}
