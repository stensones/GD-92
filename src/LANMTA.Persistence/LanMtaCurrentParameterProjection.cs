using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA.Persistence;

public sealed class LanMtaCurrentParameterProjection
{
	private readonly IReadOnlyDictionary<ParameterNumber, ParameterValue> values;

	private LanMtaCurrentParameterProjection(
		IReadOnlyDictionary<ParameterNumber, ParameterValue> values)
	{
		this.values = new Dictionary<ParameterNumber, ParameterValue>(values);
	}

	public ParameterValue Get(ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return this.TryGet(parameterNumber, out var parameterValue)
			? parameterValue
			: throw new ArgumentOutOfRangeException(
				nameof(parameterNumber),
				"LAN MTA does not own the requested Parameter.");
	}

	public bool TryGet(
		ParameterNumber parameterNumber,
		out ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return this.values.TryGetValue(parameterNumber, out parameterValue!);
	}

	public static LanMtaCurrentParameterProjection FromNonVolatileParameters(
		IReadOnlyDictionary<ParameterNumber, ParameterValue> values)
	{
		ArgumentNullException.ThrowIfNull(values);

		return new LanMtaCurrentParameterProjection(values);
	}
}
