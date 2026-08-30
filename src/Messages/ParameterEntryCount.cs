namespace Stensones.GD92.Messages;

public readonly record struct ParameterEntryCount(ushort Value)
{
	public static ParameterEntryCount FromValue(ushort value)
	{
		return new ParameterEntryCount(value);
	}
}
