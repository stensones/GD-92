using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class EnvelopeSteps
{
	private CommunicationsAddress? source;
	private Destinations? destinations;
	private ProtocolAndPriority? protocolAndPriority;
	private AcknowledgementAndSequence? acknowledgementAndSequence;
	private IGD92MessageContents? contents;
	private Envelope? envelope;
	private byte[]? encodedEnvelope;
	private Destinations? affectedDestinations;
	private InvalidOperationException? decodingException;
	private InvalidOperationException? acknowledgementException;
	private InvalidOperationException? envelopeCreationException;
	private ArgumentOutOfRangeException? oversizedEnvelopeException;

	[Given(@"an Envelope source and destination of Brigade (.*), Node (.*), and Port (.*)")]
	public void GivenAnEnvelopeSourceAndDestination(byte brigade, ushort node, byte port)
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));

		this.source = address;
		this.destinations = Destinations.FromAddresses(address);
	}

	[Given(@"an Envelope priority of (.*) and protocol version of (.*)")]
	public void GivenAnEnvelopePriorityAndProtocolVersion(byte priority, byte protocolVersion)
	{
		this.protocolAndPriority = ProtocolAndPriority.FromValues(
			MessagePriority.FromValue(MessagePriorityLevel.FromValue(priority)),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion)));
	}

	[Given(@"an Envelope sequence number of (.*) requesting acknowledgement")]
	public void GivenAnEnvelopeSequenceNumberRequestingAcknowledgement(ushort sequenceNumber)
	{
		this.acknowledgementAndSequence = AcknowledgementAndSequence.FromValues(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(sequenceNumber)),
			AcknowledgementRequest.Requested);
	}

	[Given(@"an Envelope sequence number of (.*) without requesting acknowledgement")]
	public void GivenAnEnvelopeSequenceNumberWithoutRequestingAcknowledgement(ushort sequenceNumber)
	{
		this.acknowledgementAndSequence = AcknowledgementAndSequence.FromValues(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(sequenceNumber)),
			AcknowledgementRequest.NotRequested);
	}

	[Given(@"a single-block Text message containing ""(.*)""")]
	public void GivenASingleBlockTextMessageContaining(string text)
	{
		this.contents = Text.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			Stensones.GD92.Fields.Text.FromValue(text));
	}

	[Given(@"a Mobilise Command activating station sounders and appliance indicator 1 requiring manual acknowledgement")]
	public void GivenAMobiliseCommandActivatingStationSoundersAndApplianceIndicator1RequiringManualAcknowledgement()
	{
		this.contents = MobiliseCommand.FromFields(
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1),
			ManualAcknowledgementRequest.Required);
	}

	[Given(@"an Activate Peripheral message activating station sounders and appliance indicator 1")]
	public void GivenAnActivatePeripheralMessageActivatingStationSoundersAndApplianceIndicator1()
	{
		this.contents = ActivatePeripheral.FromFields(
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));
	}

	[Given(@"a Deactivate Peripheral message deactivating station sounders and appliance indicator 1")]
	public void GivenADeactivatePeripheralMessageDeactivatingStationSoundersAndApplianceIndicator1()
	{
		this.contents = DeactivatePeripheral.FromFields(
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));
	}

	[Given(@"a Resource Status Request for resource ""(.*)""")]
	public void GivenAResourceStatusRequestForResource(string callsign)
	{
		this.contents = ResourceStatusRequest.FromCallsigns(
			Callsign.FromValue(SevenBitAsciiString.FromValue(callsign)));
	}

	[Given(@"a Peripheral Status Request")]
	public void GivenAPeripheralStatusRequest()
	{
		this.contents = PeripheralStatusRequest.Create();
	}

	[Given(@"an unsolicited Peripheral Status reporting manual acknowledgement, paper low, station sounders, and appliance indicator 1")]
	public void GivenAnUnsolicitedPeripheralStatus()
	{
		this.contents = PeripheralStatus.FromFields(
			InputPeripherals.FromInputs(
				InputPeripheral.ManualAcknowledgementPressed,
				InputPeripheral.PaperLow),
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));
	}

	[Given(@"an Alert Crew message for Firecall Team A requiring manual acknowledgement and activating station sounders and appliance indicator 1")]
	public void GivenAnAlertCrewMessageForFirecallTeamA()
	{
		this.contents = AlertCrew.FromFields(
			AlertGroup.FromValue(AlertGroupValue.FirecallTeamA),
			ManualAcknowledgementRequest.Required,
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));
	}

	[Given(@"a single-block Mobilise Message for resource ""(.*)"" submitted at ""(.*)"" for Incident (.*)")]
	public void GivenASingleBlockMobiliseMessageForResourceSubmittedAtForIncident(
		string callsign,
		string submittedAt,
		uint incidentNumber)
	{
		this.contents = MobiliseMessage.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			ManualAcknowledgementRequest.NotRequired,
			TimeAndDate.FromValue(SevenBitAsciiString.FromValue(submittedAt)),
			CallsignList.FromValues(Callsign.FromValue(SevenBitAsciiString.FromValue(callsign))),
			IncidentDetails.FromFields(
				IncidentNumber.FromValue(incidentNumber),
				MobilisationType.FromValue(MobilisationTypeValue.Incident),
				IncidentAddress.FromValues(
					AddressText.FromValue(SevenBitAsciiString.FromValue("STATION")),
					HouseNumber.FromValue(SevenBitAsciiString.FromValue("1")),
					Street.FromValue(SevenBitAsciiString.FromValue("HIGH STREET")),
					SubDistrict.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
					District.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
					Town.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
					County.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
					Postcode.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				MapReference.FromValue(SevenBitAsciiString.FromValue("SU123456")),
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));
	}

	[Given(@"a Parameter Request for the current table and parameter number (.*)")]
	public void GivenAParameterRequestForTheCurrentTable(byte parameterNumber)
	{
		this.contents = ParameterRequest.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(parameterNumber));
	}

	[Given(@"a Parameter Request Multiple for current-table parameter number (.*) and entries (.*) through (.*)")]
	public void GivenAParameterRequestMultipleForCurrentTable(
		byte parameterNumber,
		ushort firstEntry,
		ushort lastEntry)
	{
		this.contents = ParameterRequestMultiple.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(parameterNumber),
			ParameterEntrySelection.Range(
				ParameterEntryIndex.FromValue(firstEntry),
				ParameterEntryIndex.FromValue(lastEntry)));
	}

	[Given(@"a Parameter Request Multiple for current-table parameter number (.*) requesting the (.*) most recent entries")]
	public void GivenAParameterRequestMultipleForCurrentTableRequestingMostRecentEntries(
		byte parameterNumber,
		ushort entryCount)
	{
		this.contents = ParameterRequestMultiple.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(parameterNumber),
			ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(entryCount)));
	}

	[When(@"the Envelope is created")]
	public void WhenTheEnvelopeIsCreated()
	{
		this.envelope = Envelope.FromValues(
			this.source!,
			this.destinations!,
			this.protocolAndPriority!,
			this.acknowledgementAndSequence!,
			this.contents!);
	}

	[When(@"Envelope creation is attempted")]
	public void WhenEnvelopeCreationIsAttempted()
	{
		try
		{
			this.envelope = Envelope.FromValues(
				this.source!,
				this.destinations!,
				this.protocolAndPriority!,
				this.acknowledgementAndSequence!,
				this.contents!);
		}
		catch (InvalidOperationException exception)
		{
			this.envelopeCreationException = exception;
		}
	}

	[Given(@"Message Contents containing (.*) bytes")]
	public void GivenMessageContentsContainingBytes(int length)
	{
		this.contents = new SizedMessageContents(length);
	}

	[When(@"creation of the oversized Envelope is attempted")]
	public void WhenCreationOfTheOversizedEnvelopeIsAttempted()
	{
		try
		{
			this.envelope = Envelope.FromValues(
				this.source!,
				this.destinations!,
				this.protocolAndPriority!,
				this.acknowledgementAndSequence!,
				this.contents!);
		}
		catch (ArgumentOutOfRangeException exception)
		{
			this.oversizedEnvelopeException = exception;
		}
	}

	[Then(@"the Parameter Request Envelope creation is rejected")]
	public void ThenTheParameterRequestEnvelopeCreationIsRejected()
	{
		this.envelopeCreationException.Should().NotBeNull();
	}

	[Then(@"the oversized Envelope is rejected")]
	public void ThenTheOversizedEnvelopeIsRejected()
	{
		this.oversizedEnvelopeException.Should().NotBeNull();
	}

	[Then(@"its complete Envelope bytes are ""(.*)""")]
	public void ThenItsCompleteEnvelopeBytesAre(string expectedBytes)
	{
		this.envelope!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Given(@"encoded Envelope bytes ""(.*)""")]
	public void GivenEncodedEnvelopeBytes(string encodedEnvelope)
	{
		this.encodedEnvelope = Convert.FromHexString(encodedEnvelope);
	}

	[When(@"the Envelope is decoded")]
	public void WhenTheEnvelopeIsDecoded()
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);

		this.envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
	}

	[Then(@"its decoded source is Brigade (.*), Node (.*), Port (.*)")]
	public void ThenItsDecodedSourceIs(byte brigade, ushort node, byte port)
	{
		this.envelope!.Source.Should().Be(CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port))));
	}

	[Then(@"it has (.*) decoded destination")]
	public void ThenItHasDecodedDestination(byte expectedCount)
	{
		this.envelope!.Destinations.Count.Value.Should().Be(expectedCount);
	}

	[Then(@"its decoded Text Message Contents are block (.*) of (.*) containing ""(.*)""")]
	public void ThenItsDecodedTextMessageContentsAre(byte block, byte ofBlocks, string text)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<Text>().Which;

		contents.Block.Value.Should().Be(block);
		contents.OfBlocks.Value.Should().Be(ofBlocks);
		contents.MessageText.Value.Should().Be(text);
	}

	[Then(@"its decoded Mobilise Command activates station sounders and appliance indicator 1 and requires manual acknowledgement")]
	public void ThenItsDecodedMobiliseCommandActivatesStationSoundersAndApplianceIndicator1AndRequiresManualAcknowledgement()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<MobiliseCommand>().Which;

		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		contents.ManualAcknowledgementRequest.Should().Be(ManualAcknowledgementRequest.Required);
	}

	[Then(@"its decoded Activate Peripheral message activates station sounders and appliance indicator 1")]
	public void ThenItsDecodedActivatePeripheralMessageActivatesStationSoundersAndApplianceIndicator1()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<ActivatePeripheral>().Which;

		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
	}

	[Then(@"its decoded Deactivate Peripheral message deactivates station sounders and appliance indicator 1")]
	public void ThenItsDecodedDeactivatePeripheralMessageDeactivatesStationSoundersAndApplianceIndicator1()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<DeactivatePeripheral>().Which;

		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
	}

	[Then(@"its decoded Resource Status Request identifies resource ""(.*)""")]
	public void ThenItsDecodedResourceStatusRequestIdentifiesResource(string callsign)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<ResourceStatusRequest>().Which;

		contents.Callsigns.Select(value => value.Value.Value).Should().Equal(callsign);
	}

	[Then(@"its decoded Contents are a Peripheral Status Request")]
	public void ThenItsDecodedContentsAreAPeripheralStatusRequest()
	{
		this.envelope!.Contents.Should().BeOfType<PeripheralStatusRequest>();
	}

	[Then(@"its decoded Peripheral Status reports manual acknowledgement, paper low, station sounders, and appliance indicator 1")]
	public void ThenItsDecodedPeripheralStatusReportsItsInputsAndOutputs()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<PeripheralStatus>().Which;

		contents.InputPeripherals.IsInputSet(InputPeripheral.ManualAcknowledgementPressed).Should().BeTrue();
		contents.InputPeripherals.IsInputSet(InputPeripheral.PaperLow).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
	}

	[Then(@"its decoded Alert Crew message identifies Firecall Team A, requires manual acknowledgement, and activates station sounders and appliance indicator 1")]
	public void ThenItsDecodedAlertCrewMessagePreservesItsFields()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<AlertCrew>().Which;

		contents.AlertGroup.Value.Should().Be(AlertGroupValue.FirecallTeamA);
		contents.ManualAcknowledgementRequest.Should().Be(ManualAcknowledgementRequest.Required);
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		contents.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
	}

	[Then(@"its decoded Mobilise Message preserves the resource and incident details")]
	public void ThenItsDecodedMobiliseMessagePreservesTheResourceAndIncidentDetails()
	{
		var contents = this.envelope!.Contents.Should().BeOfType<MobiliseMessage>().Which;

		contents.Block.Value.Should().Be(1);
		contents.OfBlocks.Value.Should().Be(1);
		contents.ManualAcknowledgementRequest.Should().Be(ManualAcknowledgementRequest.NotRequired);
		contents.TimeAndDate.Value.Value.Should().Be("07SEP26154309");
		contents.CallsignList.Values.Select(callsign => callsign.Value.Value).Should().Equal("A1");
		contents.IncidentDetails.Should().ContainSingle();
		contents.IncidentDetails[0].IncidentNumber.Value.Should().Be(1U);
		contents.IncidentDetails[0].MobilisationType.Value.Should().Be(MobilisationTypeValue.Incident);
		contents.IncidentDetails[0].Address.AddressText.Value.Value.Should().Be("STATION");
		contents.IncidentDetails[0].MapReference.Value.Value.Should().Be("SU123456");
		contents.IncidentDetails[0].TelephoneNumber.Value.Value.Should().Be("0123");
		contents.IncidentDetails[0].Text.Value.Should().Be("FIRE");
	}

	[Then(@"its decoded Parameter Request identifies the current table and parameter number (.*)")]
	public void ThenItsDecodedParameterRequestIdentifiesTheCurrentTable(byte parameterNumber)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<ParameterRequest>().Which;

		contents.ParameterTable.Should().Be(ParameterTable.Current);
		contents.ParameterNumber.Value.Should().Be(parameterNumber);
	}

	[Then(@"its decoded Set Parameter identifies the non-volatile table, parameter number (.*), and value bytes ""(.*)""")]
	public void ThenItsDecodedSetParameterIdentifiesTheNonVolatileTable(
		byte parameterNumber,
		string parameterValue)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<SetParameter>().Which;

		contents.ParameterTable.Should().Be(ParameterTable.NonVolatile);
		contents.ParameterNumber.Value.Should().Be(parameterNumber);
		contents.ParameterValue.ToWireValue().Should().Equal(Convert.FromHexString(parameterValue));
	}

	[Then(@"its decoded Parameter Request Multiple identifies current-table parameter number (.*) and entries (.*) through (.*)")]
	public void ThenItsDecodedParameterRequestMultipleIdentifiesCurrentTable(
		byte parameterNumber,
		ushort firstEntry,
		ushort lastEntry)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<ParameterRequestMultiple>().Which;

		contents.ParameterTable.Should().Be(ParameterTable.Current);
		contents.ParameterNumber.Value.Should().Be(parameterNumber);
		contents.EntrySelection.Should().Be(ParameterEntrySelection.Range(
			ParameterEntryIndex.FromValue(firstEntry),
			ParameterEntryIndex.FromValue(lastEntry)));
	}

	[Then(@"its decoded Parameter Request Multiple requests the (.*) most recent entries")]
	public void ThenItsDecodedParameterRequestMultipleRequestsMostRecentEntries(ushort entryCount)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<ParameterRequestMultiple>().Which;

		contents.EntrySelection.Should().Be(
			ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(entryCount)));
	}

	[When(@"a Parameter Envelope is created by Brigade (.*), Node (.*), Port (.*) using protocol version (.*) returning brigade number (.*)")]
	public void WhenAParameterEnvelopeIsCreated(
		byte brigade,
		ushort node,
		byte port,
		byte protocolVersion,
		byte returnedBrigadeNumber)
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);
		var requestEnvelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		this.envelope = Envelope.CreateParameterResponse(
			requestEnvelope,
			CreateAddress(brigade, node, port),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion)),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue([returnedBrigadeNumber])));
	}

	[Then(@"its Parameter Contents contain no more values and brigade number (.*)")]
	public void ThenItsParameterContentsContainNoMoreValuesAndBrigadeNumber(byte brigadeNumber)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<Parameter>().Which;

		contents.MoreValues.Should().Be(MoreValues.No);
		contents.ParameterValue.ToWireValue().Should().Equal(new byte[] { brigadeNumber });
	}

	[When(@"decoding the Envelope is attempted")]
	public void WhenDecodingTheEnvelopeIsAttempted()
	{
		try
		{
			var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);

			this.envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
		}
		catch (InvalidOperationException exception)
		{
			this.decodingException = exception;
		}
	}

	[Then(@"the Block Check Character mismatch is rejected")]
	public void ThenTheBlockCheckCharacterMismatchIsRejected()
	{
		this.decodingException.Should().NotBeNull();
	}

	[Then(@"the Message Contents length mismatch is rejected")]
	public void ThenTheMessageContentsLengthMismatchIsRejected()
	{
		this.decodingException.Should().NotBeNull();
	}

	[Then(@"the malformed Text Contents are rejected")]
	public void ThenTheMalformedTextContentsAreRejected()
	{
		this.decodingException.Should().NotBeNull();
	}

	[Then(@"the non-empty Acknowledgement Contents are rejected")]
	public void ThenTheNonEmptyAcknowledgementContentsAreRejected()
	{
		this.decodingException.Should().NotBeNull();
	}

	[Given(@"an Envelope source of Brigade (.*), Node (.*), and Port (.*)")]
	public void GivenAnEnvelopeSource(byte brigade, ushort node, byte port)
	{
		this.source = CreateAddress(brigade, node, port);
	}

	[Given(@"Envelope destinations Brigade (.*), Node (.*), Port (.*) and Brigade (.*), Node (.*), Port (.*)")]
	public void GivenEnvelopeDestinations(
		byte firstBrigade,
		ushort firstNode,
		byte firstPort,
		byte secondBrigade,
		ushort secondNode,
		byte secondPort)
	{
		this.destinations = Destinations.FromAddresses(
			CreateAddress(firstBrigade, firstNode, firstPort),
			CreateAddress(secondBrigade, secondNode, secondPort));
	}

	[When(@"the Envelope is created and decoded")]
	public void WhenTheEnvelopeIsCreatedAndDecoded()
	{
		var envelope = Envelope.FromValues(
			this.source!,
			this.destinations!,
			this.protocolAndPriority!,
			this.acknowledgementAndSequence!,
			this.contents!);
		var buffer = new EncodedMessageBuffer(envelope.ToWireValue());

		this.envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
	}

	[Then(@"it has (.*) decoded destinations in the declared order")]
	public void ThenItHasDecodedDestinationsInTheDeclaredOrder(byte expectedCount)
	{
		this.envelope!.Destinations.Count.Value.Should().Be(expectedCount);
		this.envelope.Destinations.ToWireValue().Should().Equal(
			Convert.FromHexString("1A19191A195A"));
	}

	[Then(@"its Contents are an Acknowledgement")]
	public void ThenItsContentsAreAnAcknowledgement()
	{
		this.envelope!.Contents.Should().BeOfType<Acknowledgement>();
	}

	[Then(@"its Contents are a Negative Acknowledgement")]
	public void ThenItsContentsAreANegativeAcknowledgement()
	{
		this.envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>();
	}

	[Then(@"it identifies Brigade (.*), Node (.*), Port (.*) as the affected destination")]
	public void ThenItIdentifiesTheAffectedDestination(byte brigade, ushort node, byte port)
	{
		var negativeAcknowledgement = this.envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which;

		negativeAcknowledgement.Destinations.Addresses.Should().ContainSingle()
			.Which.Should().Be(CreateAddress(brigade, node, port));
	}

	[Then(@"its General Reason Code is ""(.*)""")]
	public void ThenItsGeneralReasonCodeIs(string expectedReasonCode)
	{
		var negativeAcknowledgement = this.envelope!.Contents.Should().BeOfType<NegativeAcknowledgement>().Which;

		negativeAcknowledgement.ReasonCode.GeneralReasonCode.Should().Be(
			Enum.Parse<GeneralReasonCode>(expectedReasonCode));
	}

	[Then(@"its Contents are preserved as Message Type (.*) with bytes ""(.*)""")]
	public void ThenItsContentsArePreservedAsMessageTypeWithBytes(byte messageType, string contents)
	{
		var unsupportedContents = this.envelope!.Contents.Should().BeOfType<UnsupportedMessageContents>().Which;

		unsupportedContents.Type.Value.Should().Be(messageType);
		unsupportedContents.ToWireValue().Should().Equal(Convert.FromHexString(contents));
	}

	private sealed class SizedMessageContents : IGD92MessageContents
	{
		public SizedMessageContents(int length)
		{
			this.Contents = new byte[length];
		}

		public byte[] Contents { get; }
		public MessageType Type => MessageType.FromValue(GD92MessageType.Text);

		public byte[] ToWireValue()
		{
			return this.Contents;
		}
	}

	[Given(@"the affected destination is Brigade (.*), Node (.*), Port (.*)")]
	public void GivenTheAffectedDestination(byte brigade, ushort node, byte port)
	{
		this.affectedDestinations = Destinations.FromAddresses(CreateAddress(brigade, node, port));
	}

	[When(@"a Negative Acknowledgement Envelope is created by Brigade (.*), Node (.*), Port (.*) using protocol version (.*) and the General Reason Code ""(.*)""")]
	public void WhenANegativeAcknowledgementEnvelopeIsCreated(
		byte brigade,
		ushort node,
		byte port,
		byte protocolVersion,
		string reasonCode)
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);
		var receivedEnvelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		this.envelope = Envelope.CreateNegativeAcknowledgement(
			receivedEnvelope,
			CreateAddress(brigade, node, port),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion)),
			this.affectedDestinations!,
			ReasonCode.FromGeneralReasonCode(ParseGeneralReasonCode(reasonCode)));
	}

	[When(@"an Acknowledgement Envelope is created by Brigade (.*), Node (.*), Port (.*) using protocol version (.*)")]
	public void WhenAnAcknowledgementEnvelopeIsCreated(
		byte brigade,
		ushort node,
		byte port,
		byte protocolVersion)
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);
		var receivedEnvelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		this.envelope = Envelope.CreateAcknowledgement(
			receivedEnvelope,
			CreateAddress(brigade, node, port),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion)));
	}

	[When(@"creating an Acknowledgement Envelope is attempted by Brigade (.*), Node (.*), Port (.*) using protocol version (.*)")]
	public void WhenCreatingAnAcknowledgementEnvelopeIsAttempted(
		byte brigade,
		ushort node,
		byte port,
		byte protocolVersion)
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);
		var receivedEnvelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		try
		{
			this.envelope = Envelope.CreateAcknowledgement(
				receivedEnvelope,
				CreateAddress(brigade, node, port),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(protocolVersion)));
		}
		catch (InvalidOperationException exception)
		{
			this.acknowledgementException = exception;
		}
	}

	[Then(@"the acknowledgement response is rejected")]
	public void ThenTheAcknowledgementResponseIsRejected()
	{
		this.acknowledgementException.Should().NotBeNull();
	}

	private static GeneralReasonCode ParseGeneralReasonCode(string reasonCode)
	{
		return reasonCode switch
		{
			"inv_mess" => GeneralReasonCode.InvalidMessage,
			_ => throw new ArgumentOutOfRangeException(nameof(reasonCode))
		};
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
