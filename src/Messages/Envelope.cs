using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class Envelope
{
	private readonly byte[] messageWireValue;

	private Envelope(
		CommunicationsAddress source,
		Destinations destinations,
		ProtocolAndPriority protocolAndPriority,
		AcknowledgementAndSequence acknowledgementAndSequence,
		IGD92Message message,
		CountAndLength countAndLength,
		byte[] messageWireValue)
	{
		this.Source = source;
		this.Destinations = destinations;
		this.ProtocolAndPriority = protocolAndPriority;
		this.AcknowledgementAndSequence = acknowledgementAndSequence;
		this.Message = message;
		this.CountAndLength = countAndLength;
		this.messageWireValue = messageWireValue;
	}

	public CommunicationsAddress Source { get; }
	public Destinations Destinations { get; }
	public ProtocolAndPriority ProtocolAndPriority { get; }
	public AcknowledgementAndSequence AcknowledgementAndSequence { get; }
	public IGD92Message Message { get; }
	public CountAndLength CountAndLength { get; }

	public static Envelope FromValues(
		CommunicationsAddress source,
		Destinations destinations,
		ProtocolAndPriority protocolAndPriority,
		AcknowledgementAndSequence acknowledgementAndSequence,
		IGD92Message message)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destinations);
		ArgumentNullException.ThrowIfNull(protocolAndPriority);
		ArgumentNullException.ThrowIfNull(acknowledgementAndSequence);
		ArgumentNullException.ThrowIfNull(message);

		var messageWireValue = message.ToWireValue();
		ArgumentNullException.ThrowIfNull(messageWireValue);

		if (messageWireValue.Length > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(message));
		}

		var countAndLength = CountAndLength.FromValues(
			MessageLength.FromValue((ushort)messageWireValue.Length),
			destinations.Count);

		return new Envelope(
			source,
			destinations,
			protocolAndPriority,
			acknowledgementAndSequence,
			message,
			countAndLength,
			messageWireValue.ToArray());
	}

	public byte[] ToWireValue()
	{
		var envelopeBytesBeforeBlockCheckCharacter = new List<byte>();
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Source.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.CountAndLength.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Destinations.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.ProtocolAndPriority.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.AcknowledgementAndSequence.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.Message.Type.ToWireValue());
		envelopeBytesBeforeBlockCheckCharacter.AddRange(this.messageWireValue);
		var blockCheckCharacter = BlockCheckCharacter.FromEnvelopeBytes(
			envelopeBytesBeforeBlockCheckCharacter.ToArray());

		return [.. envelopeBytesBeforeBlockCheckCharacter, .. blockCheckCharacter.ToWireValue()];
	}
}
