namespace Stensones.GD92.Fields;

public sealed record CallsignList : IGD9Field
{
	private const int CountBitCount = 8;
	private const string NullCallsignMessage = "Callsigns may not contain null values.";

	private CallsignList(IReadOnlyList<Callsign> values)
	{
		this.Values = values;
	}

	public IReadOnlyList<Callsign> Values { get; }

	public static CallsignList FromValues(params Callsign[] values)
	{
		ArgumentNullException.ThrowIfNull(values);

		if (values.Length > byte.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(values));
		}

		if (values.Any(value => value is null))
		{
			throw new ArgumentException(NullCallsignMessage, nameof(values));
		}

		return new CallsignList(Array.AsReadOnly(values.ToArray()));
	}

	public static CallsignList FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var count = (byte)buffer.ReadUnsignedBits(CountBitCount);
		var values = new Callsign[count];

		for (var index = 0; index < values.Length; index++)
		{
			values[index] = Callsign.FromEncodedMessageBuffer(ref buffer);
		}

		return FromValues(values);
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte> { (byte)this.Values.Count };

		foreach (var value in this.Values)
		{
			wireValue.AddRange(value.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
