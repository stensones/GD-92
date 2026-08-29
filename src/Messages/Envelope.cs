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

	public static Envelope FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var source = CommunicationsAddress.FromEncodedMessageBuffer(ref buffer);
		var countAndLength = CountAndLength.FromEncodedMessageBuffer(ref buffer);
		var destinations = Destinations.FromEncodedMessageBuffer(
			ref buffer,
			countAndLength.DestinationCount);
		var protocolAndPriority = ProtocolAndPriority.FromEncodedMessageBuffer(ref buffer);
		var acknowledgementAndSequence = AcknowledgementAndSequence.FromEncodedMessageBuffer(ref buffer);
		var messageType = MessageType.FromEncodedMessageBuffer(ref buffer);
		var contentsWireValue = ReadBytes(ref buffer, countAndLength.MessageLength.Value);
		var contents = DecodeContents(messageType, contentsWireValue);
		var receivedBlockCheckCharacter = BlockCheckCharacter.FromEncodedMessageBuffer(ref buffer);

		var envelope = new Envelope(
			source,
			destinations,
			protocolAndPriority,
			acknowledgementAndSequence,
			contents,
			countAndLength,
			contentsWireValue);
		var expectedBlockCheckCharacter = BlockCheckCharacter.FromEnvelopeBytes(
			envelope.GetBytesBeforeBlockCheckCharacter());

		if (receivedBlockCheckCharacter != expectedBlockCheckCharacter)
		{
			throw new InvalidOperationException("The encoded Envelope Block Check Character does not match.");
		}

		return envelope;
	}

	public byte[] ToWireValue()
	{
		var envelopeBytesBeforeBlockCheckCharacter = this.GetBytesBeforeBlockCheckCharacter();
		var blockCheckCharacter = BlockCheckCharacter.FromEnvelopeBytes(
			envelopeBytesBeforeBlockCheckCharacter);

		return [.. envelopeBytesBeforeBlockCheckCharacter, .. blockCheckCharacter.ToWireValue()];
	}

	private byte[] GetBytesBeforeBlockCheckCharacter()
	{
		return [
			.. this.Source.ToWireValue(),
			.. this.CountAndLength.ToWireValue(),
			.. this.Destinations.ToWireValue(),
			.. this.ProtocolAndPriority.ToWireValue(),
			.. this.AcknowledgementAndSequence.ToWireValue(),
			.. this.Contents.Type.ToWireValue(),
			.. this.contentsWireValue
		];
	}

	private static IGD92MessageContents DecodeContents(MessageType messageType, byte[] contentsWireValue)
	{
		var contentsBuffer = new EncodedMessageBuffer(contentsWireValue);
		IGD92MessageContents contents = (GD92MessageType)messageType.Value switch
		{
			GD92MessageType.Text => Text.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Acknowledgement => Acknowledgement.FromEncodedMessageBuffer(ref contentsBuffer),
			_ => throw new NotSupportedException($"Message Type {messageType.Value} is not supported.")
		};

		if (contentsBuffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("The encoded Message Contents length does not match its Message Type.");
		}

		return contents;
	}

	private static byte[] ReadBytes(ref EncodedMessageBuffer buffer, ushort length)
	{
		var bytes = new byte[length];

		for (var index = 0; index < bytes.Length; index++)
		{
			bytes[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		return bytes;
	}
}
