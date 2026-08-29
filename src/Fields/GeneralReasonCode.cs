namespace Stensones.GD92.Fields;

public enum GeneralReasonCode : byte
{
	NoBearer = 1,
	InvalidParameter = 2,
	InvalidMessage = 3,
	AvailabilityFailure = 4,
	StatusFailure = 5,
	TextTypeNotSupported = 6,
	InvalidCallsign = 7,
	NotAccepted = 8,
	WaitForAcknowledgement = 9,
	TestFailure = 10,
	InvalidTest = 11,
	InvalidProtocol = 12,
	CheckError = 13,
	Abort = 14,
	NoPort = 15,
	DataFailure = 16,
	ProformaFailure = 17
}
