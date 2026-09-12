namespace Stensones.GD92.Fields;

public ref struct EncodedMessageBuffer
{
	private const int BitsPerByte = 8;
	private const int MinimumReadableBitCount = 1;
	private const int MaximumReadableBitCount = sizeof(uint) * BitsPerByte;
	private const int MostSignificantBitOffset = BitsPerByte - 1;
	private const int SingleBitShift = 1;
	private const uint SingleBitMask = 1;
	private const string InsufficientBitsMessage = "The encoded message buffer does not contain enough bits.";

	private readonly ReadOnlySpan<byte> payload;
	private int bitPosition;

	public EncodedMessageBuffer(ReadOnlySpan<byte> payload, int bitPosition = 0)
	{
		var payloadBitCount = checked(payload.Length * BitsPerByte);

		if (bitPosition < 0 || bitPosition > payloadBitCount)
		{
			throw new ArgumentOutOfRangeException(nameof(bitPosition));
		}

		this.payload = payload;
		this.bitPosition = bitPosition;
	}

	public int BitPosition => this.bitPosition;

	public int ByteOffset => this.bitPosition / BitsPerByte;

	public int BitOffsetInByte => this.bitPosition % BitsPerByte;

	public int RemainingBitCount => checked(this.payload.Length * BitsPerByte) - this.bitPosition;

	public uint ReadUnsignedBits(int bitCount)
	{
		if (bitCount is < MinimumReadableBitCount or > MaximumReadableBitCount)
		{
			throw new ArgumentOutOfRangeException(nameof(bitCount));
		}

		if (bitCount > this.RemainingBitCount)
		{
			throw new InvalidOperationException(InsufficientBitsMessage);
		}

		var value = 0U;

		for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
		{
			var payloadBitPosition = this.bitPosition + bitIndex;
			var byteOffset = payloadBitPosition / BitsPerByte;
			var bitOffsetInByte = payloadBitPosition % BitsPerByte;
			var bit = (uint)((this.payload[byteOffset] >> (MostSignificantBitOffset - bitOffsetInByte)) & SingleBitMask);

			value = (value << SingleBitShift) | bit;
		}

		this.bitPosition += bitCount;

		return value;
	}
}
