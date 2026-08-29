using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class Envelope
{
	private readonly byte[] contentsWireValue;

	private Envelope(
		CommunicationsAddress source,
		Destinations destinations,
		ProtocolAndPriority protocolAndPriority,
		AcknowledgementAndSequence acknowledgementAndSequence,
		IGD92MessageContents contents,
		CountAndLength countAndLength,
		byte[] contentsWireValue)
	{
		this.Source = source;
		this.Destinations = destinations;
		this.ProtocolAndPriority = protocolAndPriority;
		this.AcknowledgementAndSequence = acknowledgementAndSequence;
		this.Contents = contents;
		this.CountAndLength = countAndLength;
		this.contentsWireValue = contentsWireValue;
	}

	public CommunicationsAddress Source { get; }
	public Destinations Destinations { get; }
	public ProtocolAndPriority ProtocolAndPriority { get; }
	public AcknowledgementAndSequence AcknowledgementAndSequence { get; }
	public IGD92MessageContents Contents { get; }
	public CountAndLength CountAndLength { get; }

	public static Envelope FromValues(
		CommunicationsAddress source,
		Destinations destinations,
		ProtocolAndPriority protocolAndPriority,
		AcknowledgementAndSequence acknowledgementAndSequence,
		IGD92MessageContents contents)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destinations);
		ArgumentNullException.ThrowIfNull(protocolAndPriority);
		ArgumentNullException.ThrowIfNull(acknowledgementAndSequence);
		ArgumentNullException.ThrowIfNull(contents);

		var contentsWireValue = contents.ToWireValue();
		ArgumentNullException.ThrowIfNull(contentsWireValue);

		if (contentsWireValue.Length > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(contents));
		}

		var countAndLength = CountAndLength.FromValues(
			MessageLength.FromValue((ushort)contentsWireValue.Length),
			destinations.Count);

		return new Envelope(
			source,
			destinations,
			protocolAndPriority,
			acknowledgementAndSequence,
			contents,
			countAndLength,
			contentsWireValue.ToArray());
	}

	public byte[] ToWireValue()
	{
		var envelopeBytesBeforeBlockCheckCharacter = new List<byte>();
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Source.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.CountAndLength.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Destinations.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.ProtocolAndPriority.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.AcknowledgementAndSequence.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Contents.Type.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.contentsWireValue);
		var blockCheckCharacter = BlockCheckCharacter.FromEnvelopeBytes(
			envelopeBytesBeforeBlockCheckCharacter.ToArray());

		return [.. envelopeBytesBeforeBlockCheckCharacter, .. blockCheckCharacter.ToWireValue()];
	}
}
