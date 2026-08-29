using System.Text;

namespace Stensones.GD92.Fields;

public sealed record Text : IGD9Field
{
	private static readonly Encoding Ascii = Encoding.GetEncoding(
		"us-ascii",
		EncoderFallback.ExceptionFallback,
		DecoderFallback.ExceptionFallback);

	private readonly byte[] value;

	private Text(byte[] value)
	{
		this.value = value;
	}

	public static Text FromValue(string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var encodedValue = Compress(Ascii.GetBytes(value));

		if (encodedValue.Length > ushort.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Text(encodedValue);
	}

	public byte[] ToWireValue()
	{
		var wireValue = new byte[this.value.Length + 2];
		wireValue[0] = (byte)(this.value.Length >> 8);
		wireValue[1] = (byte)this.value.Length;
		this.value.CopyTo(wireValue, 2);

		return wireValue;
	}

	private static byte[] Compress(ReadOnlySpan<byte> value)
	{
		var compressedValue = new List<byte>();

		for (var index = 0; index < value.Length;)
		{
			var runLength = 1;

			while (index + runLength < value.Length && value[index + runLength] == value[index])
			{
				runLength++;
			}

			if (runLength > 3)
			{
				var remainingRunLength = runLength;

				while (remainingRunLength > 3)
				{
					var compressedRunLength = Math.Min(remainingRunLength, byte.MaxValue);
					compressedValue.Add(0x1B);
					compressedValue.Add(value[index]);
					compressedValue.Add((byte)compressedRunLength);
					remainingRunLength -= compressedRunLength;
				}

				for (var runIndex = 0; runIndex < remainingRunLength; runIndex++)
				{
					compressedValue.Add(value[index]);
				}
			}
			else if (value[index] == 0x1B)
			{
				for (var runIndex = 0; runIndex < runLength; runIndex++)
				{
					compressedValue.Add(0x1B);
					compressedValue.Add(0x1B);
					compressedValue.Add(0x01);
				}
			}
			else
			{
				for (var runIndex = 0; runIndex < runLength; runIndex++)
				{
					compressedValue.Add(value[index]);
				}
			}

			index += runLength;
		}

		return compressedValue.ToArray();
	}
}
