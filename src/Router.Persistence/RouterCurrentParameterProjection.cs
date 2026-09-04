using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

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

	public static RouterCurrentParameterProjection FromParameterOneValue(
		ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		var encodedValue = parameterValue.ToWireValue();
		if (encodedValue.Length != 1)
		{
			throw new ArgumentException(
				"Router Parameter 1 must contain exactly one encoded octet.",
				nameof(parameterValue));
		}

		return new RouterCurrentParameterProjection(
			BrigadeOrAgencyIdentifier.FromValue(encodedValue[0]));
	}
}
