using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record MakeUpAppliance
{
	private MakeUpAppliance(ApplianceType type, ApplianceQuantity quantity)
	{
		this.Type = type;
		this.Quantity = quantity;
	}

	public ApplianceType Type { get; }
	public ApplianceQuantity Quantity { get; }

	public static MakeUpAppliance FromFields(ApplianceType type, ApplianceQuantity quantity)
	{
		ArgumentNullException.ThrowIfNull(type);
		ArgumentNullException.ThrowIfNull(quantity);

		return new MakeUpAppliance(type, quantity);
	}

	public static MakeUpAppliance FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			ApplianceType.FromEncodedMessageBuffer(ref buffer),
			ApplianceQuantity.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Type.ToWireValue(),
			.. this.Quantity.ToWireValue()
		];
	}
}
