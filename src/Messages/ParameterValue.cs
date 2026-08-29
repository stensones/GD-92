namespace Stensones.GD92.Messages;

public sealed class ParameterValue
{
	private readonly byte[] wireValue;

	private ParameterValue(byte[] wireValue)
	{
		this.wireValue = wireValue;
	}

	public static ParameterValue FromWireValue(ReadOnlySpan<byte> wireValue)
	{
		return new ParameterValue(wireValue.ToArray());
	}

	public byte[] ToWireValue()
	{
		return this.wireValue.ToArray();
	}
}
