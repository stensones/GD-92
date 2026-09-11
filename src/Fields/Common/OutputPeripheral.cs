namespace Stensones.GD92.Fields;

public enum OutputPeripheral : ushort
{
	StationSounders = 0x0001,
	StationLights = 0x0002,
	StationDoors = 0x0020,
	StandbySounder = 0x0040,
	ApplianceIndicator1 = 0x0100,
	ApplianceIndicator2 = 0x0200,
	ApplianceIndicator3 = 0x0400,
	ApplianceIndicator4 = 0x0800,
	ApplianceIndicator5 = 0x1000,
	ApplianceIndicator6 = 0x2000,
	ApplianceIndicator7 = 0x4000,
	ApplianceIndicator8 = 0x8000
}
