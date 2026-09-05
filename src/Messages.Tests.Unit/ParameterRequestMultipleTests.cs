using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterRequestMultipleTests
{
	[Fact]
	public void Serializes_a_single_Router_table_entry_request()
	{
		var request = ParameterRequestMultiple.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(13),
			ParameterEntrySelection.Range(
				ParameterEntryIndex.FromValue(1),
				ParameterEntryIndex.FromValue(1)));

		request.Type.ToWireValue().Should().Equal(new byte[] { 0x3F });
		request.EntrySelection.Should().Be(ParameterEntrySelection.Range(
			ParameterEntryIndex.FromValue(1),
			ParameterEntryIndex.FromValue(1)));
		request.ToWireValue().Should().Equal(Convert.FromHexString("020D00010001"));
	}

	[Fact]
	public void Serializes_a_request_for_the_most_recent_Router_table_entries()
	{
		var request = ParameterRequestMultiple.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(13),
			ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(2)));

		request.ToWireValue().Should().Equal(Convert.FromHexString("020D00000002"));
	}

	[Fact]
	public void Rejects_a_range_beginning_at_entry_zero()
	{
		var createRange = () => ParameterEntrySelection.Range(
			ParameterEntryIndex.FromValue(0),
			ParameterEntryIndex.FromValue(2));

		createRange.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Rejects_a_request_for_zero_most_recent_Parameter_entries()
	{
		Action createEntryCount = () => ParameterEntryCount.FromValue(0);

		createEntryCount.Should().Throw<ArgumentOutOfRangeException>();
	}
}
