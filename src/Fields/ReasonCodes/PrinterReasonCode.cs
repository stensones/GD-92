namespace Stensones.GD92.Fields;

public enum PrinterReasonCode : byte
{
	NoPrintResponse = 1,
	OffLine = 2,
	NoPaper = 3,
	LowPaper = 4,
	NoPower = 5,
	NoConnection = 6
}
