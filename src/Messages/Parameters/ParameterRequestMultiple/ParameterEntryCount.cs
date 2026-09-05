namespace Stensones.GD92.Messages;

public sealed record ParameterEntryCount
{
	private ParameterEntryCount(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static ParameterEntryCount FromValue(ushort value)
	{
		if (value == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(value),
				"A Parameter Entry Count must be greater than zero.");
		}

		return new ParameterEntryCount(value);
	}
}
