using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public interface ParameterEntrySelection
{
	byte[] ToWireValue();

	static ParameterEntrySelection Range(ParameterEntryIndex firstEntry, ParameterEntryIndex lastEntry)
	{
		ArgumentNullException.ThrowIfNull(firstEntry);
		ArgumentNullException.ThrowIfNull(lastEntry);

		if (firstEntry.Value == 0)
		{
			throw new ArgumentException("A range must begin with a nonzero Parameter Entry Index.", nameof(firstEntry));
		}

		return new ParameterEntryRange(firstEntry, lastEntry);
	}

	static ParameterEntrySelection MostRecent(ParameterEntryCount count)
	{
		ArgumentNullException.ThrowIfNull(count);

		return new MostRecentParameterEntries(count);
	}

	static ParameterEntrySelection FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var firstEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer);
		var lastEntry = ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer);

		return firstEntry.Value == 0
			? MostRecent(ParameterEntryCount.FromValue(lastEntry.Value))
			: Range(firstEntry, lastEntry);
	}
}

internal sealed record ParameterEntryRange(
	ParameterEntryIndex FirstEntry,
	ParameterEntryIndex LastEntry) : ParameterEntrySelection
{
	public byte[] ToWireValue()
	{
		return [.. this.FirstEntry.ToWireValue(), .. this.LastEntry.ToWireValue()];
	}
}

internal sealed record MostRecentParameterEntries(
	ParameterEntryCount Count) : ParameterEntrySelection
{
	public byte[] ToWireValue()
	{
		return [0, 0, (byte)(this.Count.Value >> 8), (byte)this.Count.Value];
	}
}
