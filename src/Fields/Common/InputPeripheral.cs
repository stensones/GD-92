namespace Stensones.GD92.Fields;

public enum InputPeripheral : ushort
{
	ManualAcknowledgementPressed = 0x0001,
	PowerFailedBatteriesOn = 0x0002,
	PowerFailedStandbyGeneratorOn = 0x0004,
	RepeatLastMessagePressed = 0x0008,
	BatteriesLow = 0x0010,
	PaperLow = 0x0020
}
