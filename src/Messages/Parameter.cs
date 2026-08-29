using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class Parameter : IGD92MessageContents
{
	private static readonly MessageType ParameterMessageType =
		MessageType.FromValue(GD92MessageType.Parameter);

	private Parameter(MoreValues moreValues, ParameterValue parameterValue)
	{
		this.MoreValues = moreValues;
		this.ParameterValue = parameterValue;
	}

	public MoreValues MoreValues { get; }
	public ParameterValue ParameterValue { get; }
	public MessageType Type => ParameterMessageType;

	public static Parameter FromFields(MoreValues moreValues, ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(moreValues);
		ArgumentNullException.ThrowIfNull(parameterValue);

		return new Parameter(moreValues, parameterValue);
	}

	public static Parameter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var moreValues = MoreValues.FromEncodedMessageBuffer(ref buffer);
		var valueLength = buffer.RemainingBitCount;

		if (valueLength % 8 != 0)
		{
			throw new InvalidOperationException("Parameter Value must be byte-aligned.");
		}

		var parameterValue = new byte[valueLength / 8];

		for (var index = 0; index < parameterValue.Length; index++)
		{
			parameterValue[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		return FromFields(moreValues, ParameterValue.FromWireValue(parameterValue));
	}

	public byte[] ToWireValue()
	{
		return [.. this.MoreValues.ToWireValue(), .. this.ParameterValue.ToWireValue()];
	}
}
