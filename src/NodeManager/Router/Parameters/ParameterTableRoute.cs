using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public static class ParameterTableRoute
{
	public static bool TryParse(string value, out ParameterTable parameterTable)
	{
		parameterTable = value switch
		{
			"permanent" => ParameterTable.Permanent,
			"non-volatile" => ParameterTable.NonVolatile,
			"current" => ParameterTable.Current,
			_ => null!
		};

		return parameterTable is not null;
	}
}
