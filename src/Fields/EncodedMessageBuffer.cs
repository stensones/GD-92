namespace Stensones.GD92.Fields;

public ref struct EncodedMessageBuffer
{
	private readonly ReadOnlySpan<byte> payload;
	private int bitPosition;

	public EncodedMessageBuffer(ReadOnlySpan<byte> payload, int bitPosition = 0)
	{
		var payloadBitCount = checked(payload.Length * 8);

		if (bitPosition < 0 || bitPosition > payloadBitCount)
		{
			throw new ArgumentOutOfRangeException(nameof(bitPosition));
		}

		this.payload = payload;
		this.bitPosition = bitPosition;
	}

	public int BitPosition => this.bitPosition;

	public int ByteOffset => this.bitPosition / 8;

	public int BitOffsetInByte => this.bitPosition % 8;

	public int RemainingBitCount => checked(this.payload.Length * 8) - this.bitPosition;

	public uint ReadUnsignedBits(int bitCount)
	{
		if (bitCount is < 1 or > 32)
		{
			throw new ArgumentOutOfRangeException(nameof(bitCount));
		}

		if (bitCount > this.RemainingBitCount)
		{
			throw new InvalidOperationException("The encoded message buffer does not contain enough bits.");
		}

		var value = 0U;

		for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
		{
			var payloadBitPosition = this.bitPosition + bitIndex;
			var byteOffset = payloadBitPosition / 8;
			var bitOffsetInByte = payloadBitPosition % 8;
			var bit = (uint)((this.payload[byteOffset] >> (7 - bitOffsetInByte)) & 1);

			value = (value << 1) | bit;
		}

		this.bitPosition += bitCount;

		return value;
	}
}
