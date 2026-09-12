namespace Stensones.GD92.Fields;

public sealed record RequestCode : IGD9Field
{
	private const int RequestCodeBitCount = 8;

	private RequestCode(RequestCodeValue value)
	{
		this.Value = value;
	}

	public RequestCodeValue Value { get; }

	public static RequestCode FromValue(RequestCodeValue value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new RequestCode(value);
	}

	public static RequestCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((RequestCodeValue)buffer.ReadUnsignedBits(RequestCodeBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
