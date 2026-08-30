namespace Stensones.GD92.Fields;

public readonly record struct BrigadeOrAgencyIdentifier
{
	private BrigadeOrAgencyIdentifier(byte value)
	{
		this.EncodedOctet = value;
	}

	internal byte EncodedOctet { get; }

	public static BrigadeOrAgencyIdentifier FromValue(byte value)
	{
		return new BrigadeOrAgencyIdentifier(value);
	}
}
