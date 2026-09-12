namespace Stensones.GD92.Fields;

public enum GenerateAlarmValue : byte
{
	DoNotGenerateAlarmMessage = 0,
	GenerateAlarmOnAssertion = 1,
	GenerateAlarmOnDeassertion = 2,
	GenerateAlarmOnBoth = 3,
	GenerateAlarmIfAssertedLongerThanRegenerationTime = 4,
	GenerateAlarmOnAssertionAndEachRegenerationPeriod = 5,
	GenerateAlarmOnDeassertionAndEachRegenerationPeriod = 6,
	GenerateAlarmOnBothAndEachRegenerationPeriod = 7
}
