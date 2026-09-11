namespace Stensones.GD92.Fields;

public sealed record IncidentNumber : IGD9Field
{
	private const int WordBitCount = 32;
	private const int MostSignificantByteShift = 24;
	private const int SecondByteShift = 16;
	private const int ThirdByteShift = 8;

	private IncidentNumber(uint value)
	{
		this.Value = value;
	}

	public uint Value { get; }

	public static IncidentNumber FromValue(uint value)
	{
		return new IncidentNumber(value);
	}

	public static IncidentNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(buffer.ReadUnsignedBits(WordBitCount));
	}

	public byte[] ToWireValue()
	{
		return [
			(byte)(this.Value >> MostSignificantByteShift),
			(byte)(this.Value >> SecondByteShift),
			(byte)(this.Value >> ThirdByteShift),
			(byte)this.Value
		];
	}
}
