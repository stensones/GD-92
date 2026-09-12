namespace Stensones.GD92.Fields;

public sealed record DestinationNodes : IGD9Field
{
	private const int CountBitCount = 8;
	private const string NullAddressRangeMessage = "Address ranges may not contain null values.";

	private DestinationNodes(IReadOnlyList<AddressRange> addressRanges)
	{
		this.AddressRanges = addressRanges;
	}

	public IReadOnlyList<AddressRange> AddressRanges { get; }

	public static DestinationNodes FromAddressRanges(params AddressRange[] addressRanges)
	{
		ArgumentNullException.ThrowIfNull(addressRanges);

		if (addressRanges.Length > byte.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(addressRanges));
		}

		if (addressRanges.Any(addressRange => addressRange is null))
		{
			throw new ArgumentException(NullAddressRangeMessage, nameof(addressRanges));
		}

		return new DestinationNodes(Array.AsReadOnly(addressRanges.ToArray()));
	}

	public static DestinationNodes FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var count = (byte)buffer.ReadUnsignedBits(CountBitCount);
		var addressRanges = new AddressRange[count];

		for (var index = 0; index < addressRanges.Length; index++)
		{
			addressRanges[index] = AddressRange.FromEncodedMessageBuffer(ref buffer);
		}

		return FromAddressRanges(addressRanges);
	}

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte> { (byte)this.AddressRanges.Count };

		foreach (var addressRange in this.AddressRanges)
		{
			wireValue.AddRange(addressRange.ToWireValue());
		}

		return wireValue.ToArray();
	}
}
