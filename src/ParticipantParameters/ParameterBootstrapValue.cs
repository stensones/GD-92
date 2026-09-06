using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace ParticipantParameters;

public sealed class ParameterBootstrapValue
{
	private ParameterBootstrapValue(
		ParameterNumber parameterNumber,
		ParameterValue initialValue)
	{
		this.ParameterNumber = parameterNumber;
		this.InitialValue = initialValue;
	}

	public ParameterNumber ParameterNumber { get; }
	public ParameterValue InitialValue { get; }

	public static ParameterBootstrapValue FromValues(
		ParameterNumber parameterNumber,
		ParameterValue initialValue)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(initialValue);

		return new ParameterBootstrapValue(parameterNumber, initialValue);
	}
}
