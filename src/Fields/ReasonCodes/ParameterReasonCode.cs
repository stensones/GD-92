namespace Stensones.GD92.Fields;

public enum ParameterReasonCode : byte
{
	NoModificationAccess = 1,
	InvalidSyntax = 2,
	InvalidValue = 3,
	InvalidPassword = 4,
	InvalidTable = 5,
	InvalidParameter = 6,
	InvalidField = 7,
	InvalidEntry = 8,
	NoAccess = 9
}
