namespace Stensones.GD92.Fields;

public sealed record Brigade
{
	private Brigade(BrigadeOrAgencyIdentifier value)
	{
		this.Value = value;
	}

	public BrigadeOrAgencyIdentifier Value { get; }

	public static Brigade FromValue(BrigadeOrAgencyIdentifier value)
	{
		return new Brigade(value);
	}
}
