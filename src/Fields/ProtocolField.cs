namespace Stensones.GD92.Fields;

public sealed class ProtocolField : ISerializableProtocolField
{
	private readonly byte[] wireValue;

	private ProtocolField(byte[] wireValue)
	{
		this.wireValue = wireValue;
	}

	public static ProtocolField FromWireValue(ReadOnlySpan<byte> wireValue)
	{
		return new ProtocolField(wireValue.ToArray());
	}

	public byte[] ToWireValue()
	{
		return this.wireValue.ToArray();
	}

	public ProtocolField ToProtocolField()
	{
		return this;
	}
}

public interface ISerializableProtocolField
{
	ProtocolField ToProtocolField();
	byte[] ToWireValue();
}