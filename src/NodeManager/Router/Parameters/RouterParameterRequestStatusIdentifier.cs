using System.Diagnostics.CodeAnalysis;

namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestStatusIdentifier(UniqueSystemWideReference USWR)
{
	public override string ToString()
	{
		return this.USWR.ToString();
	}

	public static bool TryParse(
		string value,
		[NotNullWhen(true)] out RouterParameterRequestStatusIdentifier? identifier)
	{
		identifier = null;

		if (!UniqueSystemWideReference.TryParse(value, out var uswr))
		{
			return false;
		}

		identifier = new RouterParameterRequestStatusIdentifier(uswr);
		return true;
	}
}
