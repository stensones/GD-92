using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record DutyStaffingUpdate : IGD92MessageContents
{
	private static readonly MessageType DutyStaffingUpdateMessageType =
		MessageType.FromValue(GD92MessageType.DutyStaffingUpdate);

	private DutyStaffingUpdate(IReadOnlyList<DutyStaffingEntry> entries)
	{
		this.Entries = entries;
	}

	public IReadOnlyList<DutyStaffingEntry> Entries { get; }
	public MessageType Type => DutyStaffingUpdateMessageType;

	public static DutyStaffingUpdate FromEntries(params DutyStaffingEntry[] entries)
	{
		ArgumentNullException.ThrowIfNull(entries);

		if (entries.Any(entry => entry is null))
		{
			throw new ArgumentException("Duty staffing entries may not contain null values.", nameof(entries));
		}

		return new DutyStaffingUpdate(Array.AsReadOnly(entries.ToArray()));
	}

	public static DutyStaffingUpdate FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<DutyStaffingEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(DutyStaffingEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte>();

		foreach (var entry in this.Entries)
		{
			wireValue.AddRange(entry.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
