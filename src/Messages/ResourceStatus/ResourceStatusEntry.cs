using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ResourceStatusEntry
{
	private ResourceStatusEntry(
		Callsign callsign,
		AvlType avlType,
		AvlData avlData,
		StatusCode statusCode,
		Remarks remarks)
	{
		this.Callsign = callsign;
		this.AvlType = avlType;
		this.AvlData = avlData;
		this.StatusCode = statusCode;
		this.Remarks = remarks;
	}

	public Callsign Callsign { get; }
	public AvlType AvlType { get; }
	public AvlData AvlData { get; }
	public StatusCode StatusCode { get; }
	public Remarks Remarks { get; }

	public static ResourceStatusEntry FromFields(
		Callsign callsign,
		AvlType avlType,
		AvlData avlData,
		StatusCode statusCode,
		Remarks remarks)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(avlType);
		ArgumentNullException.ThrowIfNull(avlData);
		ArgumentNullException.ThrowIfNull(statusCode);
		ArgumentNullException.ThrowIfNull(remarks);

		if (avlType.Value == AvlTypeValue.NoAvlDataSystemPresent && avlData.Value.Value.Length != 0)
		{
			throw new ArgumentException("AVL Data must be empty when no AVL data system is present.", nameof(avlData));
		}

		return new ResourceStatusEntry(callsign, avlType, avlData, statusCode, remarks);
	}

	public static ResourceStatusEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			Callsign.FromEncodedMessageBuffer(ref buffer),
			AvlType.FromEncodedMessageBuffer(ref buffer),
			AvlData.FromEncodedMessageBuffer(ref buffer),
			StatusCode.FromEncodedMessageBuffer(ref buffer),
			Remarks.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Callsign.ToWireValue(),
			.. this.AvlType.ToWireValue(),
			.. this.AvlData.ToWireValue(),
			.. this.StatusCode.ToWireValue(),
			.. this.Remarks.ToWireValue()
		];
	}
}
