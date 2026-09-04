using Stensones.GD92.Fields;

namespace Router;

public sealed class RouterCurrentParameterProjection
{
	private RouterCurrentParameterProjection(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		this.BrigadeOrAgencyIdentifier = brigadeOrAgencyIdentifier;
	}

	public BrigadeOrAgencyIdentifier BrigadeOrAgencyIdentifier { get; }

	public static RouterCurrentParameterProjection FromBrigadeOrAgencyIdentifier(
		BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		return new RouterCurrentParameterProjection(brigadeOrAgencyIdentifier);
	}
}
