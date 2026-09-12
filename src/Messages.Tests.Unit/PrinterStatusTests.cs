using AwesomeAssertions;
using Stensones.GD92.Fields;
using FieldPrinterStatus = Stensones.GD92.Fields.PrinterStatus;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class PrinterStatusTests
{
	[Fact]
	public void Serializes_the_printer_status()
	{
		var printerStatus = global::Stensones.GD92.Messages.PrinterStatus.FromFields(
			FieldPrinterStatus.FromValue(PrinterStatusValue.Offline));

		printerStatus.ToWireValue().Should().Equal(Convert.FromHexString("00"));
	}
}
