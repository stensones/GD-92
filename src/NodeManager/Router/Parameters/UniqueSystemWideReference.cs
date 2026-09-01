using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record UniqueSystemWideReference(
	CommunicationsAddress Source,
	CommunicationsAddress Destination,
	SequenceNumber SequenceNumber)
{
	public override string ToString()
	{
		return string.Join(
			"-",
			AddressToString(this.Source),
			AddressToString(this.Destination),
			this.SequenceNumber.Value.ToString(CultureInfo.InvariantCulture));
	}

	public static bool TryParse(
		string value,
		[NotNullWhen(true)] out UniqueSystemWideReference? uswr)
	{
		uswr = null;

		var segments = value.Split('-');
		if (segments.Length != 3 ||
			!TryParseAddress(segments[0], out var source) ||
			!TryParseAddress(segments[1], out var destination) ||
			!ushort.TryParse(segments[2], CultureInfo.InvariantCulture, out var sequence) ||
			sequence > 32767)
		{
			return false;
		}

		uswr = new UniqueSystemWideReference(
			source,
			destination,
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(sequence)));
		return true;
	}

	private static string AddressToString(CommunicationsAddress address)
	{
		return string.Join(
			".",
			address.Brigade.Value.ToString(),
			address.Node.Value.ToString(CultureInfo.InvariantCulture),
			address.Port.Value.ToString(CultureInfo.InvariantCulture));
	}

	private static bool TryParseAddress(
		string value,
		[NotNullWhen(true)] out CommunicationsAddress? address)
	{
		address = null;

		var segments = value.Split('.');
		if (segments.Length != 3 ||
			!byte.TryParse(segments[0], CultureInfo.InvariantCulture, out var brigade) ||
			!ushort.TryParse(segments[1], CultureInfo.InvariantCulture, out var node) ||
			node > 1023 ||
			!byte.TryParse(segments[2], CultureInfo.InvariantCulture, out var port) ||
			port > 63)
		{
			return false;
		}

		address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
		return true;
	}
}
