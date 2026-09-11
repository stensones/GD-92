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

		if (contents is ParameterRequest or ParameterRequestMultiple or SetParameter &&
			!acknowledgementAndSequence.AcknowledgementRequest.IsRequested)
		{
			throw new InvalidOperationException("A Parameter request or Set Parameter Envelope must request acknowledgement.");
		}

		var contentsWireValue = contents.ToWireValue();
		ArgumentNullException.ThrowIfNull(contentsWireValue);

		if (contentsWireValue.Length > 1023)
		{
			throw new ArgumentOutOfRangeException(nameof(contents));
		}

		var countAndLength = CountAndLength.FromValues(
			MessageLength.FromValue(MessageByteLength.FromValue((ushort)contentsWireValue.Length)),
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

	public static Envelope CreateAcknowledgement(
		Envelope receivedEnvelope,
		CommunicationsAddress respondingSource,
		ProtocolVersion protocolVersion)
	{
		ArgumentNullException.ThrowIfNull(receivedEnvelope);
		ArgumentNullException.ThrowIfNull(respondingSource);
		ArgumentNullException.ThrowIfNull(protocolVersion);

		if (!receivedEnvelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested)
		{
			throw new InvalidOperationException("An acknowledgement can only respond to an acknowledgement-requested Envelope.");
		}

		return FromValues(
			respondingSource,
			Destinations.FromAddresses(receivedEnvelope.Source),
			ProtocolAndPriority.FromValues(
				receivedEnvelope.ProtocolAndPriority.Priority,
				protocolVersion),
			AcknowledgementAndSequence.FromValues(
				receivedEnvelope.AcknowledgementAndSequence.SequenceNumber,
				AcknowledgementRequest.NotRequested),
			Acknowledgement.Create());
	}

	public static Envelope CreateNegativeAcknowledgement(
		Envelope receivedEnvelope,
		CommunicationsAddress respondingSource,
		ProtocolVersion protocolVersion,
		Destinations affectedDestinations,
		ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(receivedEnvelope);
		ArgumentNullException.ThrowIfNull(respondingSource);
		ArgumentNullException.ThrowIfNull(protocolVersion);
		ArgumentNullException.ThrowIfNull(affectedDestinations);
		ArgumentNullException.ThrowIfNull(reasonCode);

		if (!receivedEnvelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested)
		{
			throw new InvalidOperationException("A negative acknowledgement can only respond to an acknowledgement-requested Envelope.");
		}

		return FromValues(
			respondingSource,
			Destinations.FromAddresses(receivedEnvelope.Source),
			ProtocolAndPriority.FromValues(
				receivedEnvelope.ProtocolAndPriority.Priority,
				protocolVersion),
			AcknowledgementAndSequence.FromValues(
				receivedEnvelope.AcknowledgementAndSequence.SequenceNumber,
				AcknowledgementRequest.NotRequested),
			NegativeAcknowledgement.FromValues(affectedDestinations, reasonCode));
	}

	public static Envelope CreateParameterResponse(
		Envelope requestEnvelope,
		CommunicationsAddress respondingSource,
		ProtocolVersion protocolVersion,
		Parameter parameter)
	{
		ArgumentNullException.ThrowIfNull(requestEnvelope);
		ArgumentNullException.ThrowIfNull(respondingSource);
		ArgumentNullException.ThrowIfNull(protocolVersion);
		ArgumentNullException.ThrowIfNull(parameter);

		if (requestEnvelope.Contents is not ParameterRequest)
		{
			throw new InvalidOperationException("A Parameter response can only respond to a Parameter Request Envelope.");
		}

		return FromValues(
			respondingSource,
			Destinations.FromAddresses(requestEnvelope.Source),
			ProtocolAndPriority.FromValues(
				requestEnvelope.ProtocolAndPriority.Priority,
				protocolVersion),
			AcknowledgementAndSequence.FromValues(
				requestEnvelope.AcknowledgementAndSequence.SequenceNumber,
				AcknowledgementRequest.NotRequested),
			parameter);
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
			GD92MessageType.MobiliseCommand => MobiliseCommand.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.MobiliseMessage => MobiliseMessage.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ActivatePeripheral => ActivatePeripheral.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.DeactivatePeripheral => DeactivatePeripheral.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ResourceStatusRequest => ResourceStatusRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PeripheralStatusRequest => PeripheralStatusRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PeripheralStatus => PeripheralStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Text => Text.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Acknowledgement => Acknowledgement.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.NegativeAcknowledgement => NegativeAcknowledgement.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.SetParameter => SetParameter.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ParameterRequest => ParameterRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Parameter => Parameter.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ParameterRequestMultiple => ParameterRequestMultiple.FromEncodedMessageBuffer(ref contentsBuffer),
			_ => UnsupportedMessageContents.FromWireValue(messageType, contentsWireValue)
		};

		if (contents is not UnsupportedMessageContents && contentsBuffer.RemainingBitCount != 0)
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
