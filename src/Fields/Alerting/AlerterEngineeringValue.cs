namespace Stensones.GD92.Fields;

public enum AlerterEngineeringValue : byte
{
	LockSystemToTransmitterA = 0x41,
	LockSystemToTransmitterB = 0x42,
	RestoreAlternateMessageTransmitKeying = 0x5A,
	ContinuousTransmissionCarrierOnly = 0x4A,
	CeaseContinuousCarrier = 0x4B,
	ContinuousTransmissionRepeatEngineeringCodeAddress = 0x4C,
	CeaseEngineeringCodewordTransmission = 0x4D,
	UserDefinedParameterC = 0x43,
	UserDefinedParameterD = 0x44,
	UserDefinedParameterE = 0x45
}
