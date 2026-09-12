# GD-92 Communications

This context names the concepts used by the GD-92 communications infrastructure for Fire Service Mobilising Systems. It includes protocol-visible operational concepts but not equipment-specific or full mobilisation-workflow semantics.

## Language

**GD-92 Protocol**:
The communications protocol specified by GD-92/1003A/2.2 for interoperable Fire Service Mobilising System elements.
_Avoid_: system model, implementation

### Network and Participants

**Communications Node**:
A peer participant in the GD-92 communications network representing a control-room subsystem, fire-station equipment, or appliance equipment.
_Avoid_: control-room node, station node, appliance node

**Outstation**:
A Communications Node representing fire-station equipment or appliance equipment.
_Avoid_: control-room subsystem, all Communications Nodes

**Brigade or Agency**:
The organization namespace identified by the first component of a Communications Address.
_Avoid_: organizational aggregate, Communications Node

**Brigade or Agency Identifier**:
The full-octet protocol identity of a Brigade or Agency.
_Avoid_: arbitrary byte, router queue name

**Communications Entity (CE)**:
A Router or Message Transfer Agent that transfers messages across the communications network without interpreting ordinary message contents.
_Avoid_: user agent, endpoint

**Router**:
A Communications Entity that orderly switches messages between User Agents and Message Transfer Agents within a Communications Node.
_Avoid_: communications node, Message Transfer Agent

**Router Ingress**:
The submission of an outgoing Envelope from a User Agent to its local Router for routing.
_Avoid_: final delivery, Message Transfer Agent Frame

**User-Agent Ingress**:
The delivery of an Envelope from a local Router to the addressed User Agent.
_Avoid_: browser response, Message Transfer Agent Frame

**Local Participant Ingress**:
The delivery of a management Envelope from a local Router to an addressed Message Transfer Agent or User Agent.
_Avoid_: User-Agent Ingress, browser response

**Ingress Envelope Transport**:
The RabbitMQ transport of one encoded Envelope through Router Ingress, User-Agent Ingress, or Local Participant Ingress, including wire validation and endpoint submission.
_Avoid_: Router delivery classification, Message Transfer Agent Frame

**Local Delivery Classification**:
The Router-owned decision that directs a locally addressed Envelope to Router handling, User-Agent Ingress, or Local Participant Ingress.
_Avoid_: RabbitMQ route, queue type

**Router Local Delivery**:
The Router-owned Module that classifies and delivers a locally addressed Envelope through Router handling, User-Agent Ingress, or Local Participant Ingress.
_Avoid_: Router Ingress, message forwarding

**Message Transfer Agent (MTA)**:
A Communications Entity that operates with a paired MTA to transfer messages reliably across a communications bearer.
_Avoid_: router, User Agent

**User Agent (UA)**:
The ultimate source or destination of user messages that interprets message contents while verifying source, destination, and sequence.
_Avoid_: Communications Entity, endpoint device

### User-Agent Capabilities and Management

**UA Capability**:
A User Agent's declared ability to accept or generate specified Message Types.
_Avoid_: physical device feature, bearer capability

**External System**:
A system outside the Standing Offer that presents one or more User Agents through a combined MTA and UA interface.
_Avoid_: external bearer, anonymous transport

**External Bearer**:
A communications bearer outside the Standing Offer that transports Messages without acting as a Message source or destination.
_Avoid_: User Agent, External System

**Network Manager**:
The primary logical management role addressed through a special User Agent address.
_Avoid_: dedicated network-management terminal

**Alternative Network Manager**:
The fallback logical management role addressed through a second special User Agent address.
_Avoid_: backup device

**Network Management User Agent**:
A User Agent whose Agent Type identifies it as a network-management terminal.
_Avoid_: Network Manager, Router

### Configuration

**Parameter**:
A protocol-defined configuration value owned by one Router, Message Transfer Agent, or User Agent port. The addressed participant validates, changes, and, when durable, persists its own Parameter.
_Avoid_: application setting, global configuration

**Parameter Table**:
The permanent, non-volatile, or current table containing one participant's protocol Parameters.
_Avoid_: database table, arbitrary collection

**Permanent Parameter Table**:
The immutable Parameter Table owned by a participant that supplies installed baseline values and fallback configuration.

**Non-Volatile Parameter Table**:
The modifiable Parameter Table owned by a participant whose values survive a Communications Node power-down.

**Current Parameter Table**:
The volatile Parameter Table containing the values presently used by its owning participant.

**Participant Parameter Store**:
The application-owned persistent store for one Router, Message Transfer Agent, or User Agent's Permanent and Non-Volatile Parameter Tables. Participant Parameter Stores may share physical database infrastructure but never share Parameter Table ownership.
_Avoid_: shared parameters database, Router database for all ports

**Node Parameter Set**:
The complete coordinated set of Router, Message Transfer Agent, and User Agent Parameters maintained at one Communications Node.
_Avoid_: application configuration, Router-only settings

**Communications Node Inventory**:
The discovered Router, Message Transfer Agents, and User Agents at one Communications Node, identified by port and agent type.
_Avoid_: configured list, device list

**Agent Type**:
The protocol-defined or user-defined identifier that characterizes a Message Transfer Agent or User Agent port.
_Avoid_: participant kind, physical equipment type

**Unclassified Participant**:
A discovered port whose user-defined Agent Type does not establish whether it is a Message Transfer Agent or User Agent.
_Avoid_: unknown device, Message Transfer Agent, User Agent

**Inventory Scan**:
A bounded sequence of Parameter Requests that discovers the participants of one Communications Node.
_Avoid_: configuration read, network scan

**Inventory Scan Summary**:
The outcome counts for successful and unsuccessful Participant discovery requests in one Inventory Scan.
_Avoid_: configured participant count, error log

**Parameter Number**:
The eight-bit identifier of a Parameter within a Parameter Table.
_Avoid_: Message Sequence Number, parameter value

**Parameter Entry Index**:
The sixteen-bit identifier of an entry within a table-valued Parameter.
_Avoid_: Parameter Number, database row key

**Parameter Entry Count**:
The sixteen-bit requested-entry count encoded in a Parameter Request Multiple when the most recent entries are requested.
_Avoid_: Parameter Entry Index, table capacity

**Parameter Entry Selection**:
The selection in a Parameter Request Multiple that is either an inclusive nonzero Parameter Entry Index range or a requested count of the most recent entries.
_Avoid_: two interchangeable indexes, pagination token

**Parameter Request**:
The Message Type 61 Contents that requests the value of one Parameter by its Parameter Table and Parameter Number and requires an application response.
_Avoid_: Parameter value, Set Parameter

**Parameter Request Multiple**:
The Message Type 63 Contents that requests a Parameter Entry Selection from one table-valued Parameter.
_Avoid_: Parameter Request, Parameter Message

**Set Parameter**:
The Message Type 60 Contents that requests a change to one Parameter in an addressed participant's Parameter Table.
_Avoid_: Parameter Request, Parameter Value

**More Values**:
The boolean field in a Parameter Message indicating whether additional Parameter values remain to be returned.
_Avoid_: parameter count, pagination token

**Parameter Value**:
The parameter-specific encoded value returned by a Parameter Message.
_Avoid_: Parameter Number, generic configuration value

**Parameter Message**:
The Message Type 62 Contents that returns a Parameter Value in response to a Parameter Request.
_Avoid_: Parameter, Parameter Request

**Password Level**:
The access threshold required to modify a Parameter; Level 0 is unauthenticated and Level 1 is the first protected threshold.
_Avoid_: user role, permanent authentication

**Node Login**:
The Router-owned, temporary authenticated state of one User-Agent Communications Address that authorizes Parameter modification at one Communications Node.
_Avoid_: system-wide login, device session, operator account

**Management Transaction**:
A NodeManager-initiated local Router request comprising an outgoing management Envelope, response correlation, retry and timeout behavior, and a terminal outcome.
A Node Login, local Router Parameter Request, and each Inventory Scan probe are Management Transactions.
_Avoid_: HTTP request, RabbitMQ delivery

**Router Parameter Read**:
Router-owned processing of a Current Parameter Request that validates the addressed Router Parameter and returns its Current Parameter Value.
_Avoid_: persistence query, generic Parameter handler

**Level 1 Password Modification**:
Router-owned authorization, validation, verifier creation, and optional Non-Volatile persistence for a change to the Level 1 Password.
_Avoid_: Node Login, raw password storage

**Current Password**:
Router Parameter 4, whose volatile Current value represents the single active Node Login's Password Level and supplied User-Agent Communications Address.
_Avoid_: durable password, browser session

**Level 1 Password**:
Router Parameter 5, the access password that authenticates a Level 1 Node Login.
An active Level 1 Node Login may change its Current value immediately or its Non-Volatile value for the next Router start; its Permanent value has no modification access.
Router persistence stores a salted password verifier, never the raw password.
_Avoid_: operator account password, Current Password

**Password Parameter**:
The `<pass_param>` value containing a Password Level, Password, and supplied User-Agent Communications Address.
_Avoid_: Current Password, password verifier

**No Acknowledgement Timeout**:
Router Parameter 12, the number of seconds a sending User-Agent waits for an acknowledgement before resending an unacknowledged Message.
_Avoid_: RabbitMQ delivery timeout, manual acknowledgement timeout

**Retries**:
Router Parameter 19, the maximum total attempts a sending User-Agent makes to send an unacknowledged Message.
_Avoid_: transport retry, Message Sequence count

### Message Structure and Classification

**Protocol Version**:
The version of the GD-92 protocol used to encode a Message.
_Avoid_: node deployment version, software version

**ProtocolAndPriority**:
The packed Envelope field containing a Message Priority and Protocol Version.

**CountAndLength**:
The packed Envelope field containing a Message Length and Destination Count.

**Message Length**:
The 10-bit count of bytes in an Envelope's Message Contents, ranging from 0 to 1,023.

**Destination Count**:
The 6-bit count of destination Communications Addresses in an Envelope, ranging from 1 to 63.

**Destinations**:
The ordered, unique collection of Communications Addresses to which an Envelope is sent.
_Avoid_: arbitrary address list, destination range

**Frame**:
The Message Transfer Agent-to-Message Transfer Agent transfer unit containing one encoded Envelope and bearer-protocol overhead.
_Avoid_: Envelope, Contents

**Block Check Character (BCC)**:
The one-byte value calculated by exclusive-ORing all preceding Envelope and Message Contents bytes to verify end-to-end Message integrity.
_Avoid_: frame checksum, bearer validation

**Message Sequence**:
An ordered set of independently sequenced Messages that conveys content too long for one protocol Message.
_Avoid_: frame fragmentation, oversized Message

**Message Priority**:
The ordered delivery urgency of a Message, ranging from 1 (highest) to 9 (lowest).
_Avoid_: queue hint, business severity

**AcknowledgementAndSequence**:
The packed Envelope field containing an Acknowledgement Request and Sequence Number.

**Message**:
The conceptual basic unit of network communication represented on the wire by an Envelope containing Contents.
_Avoid_: envelope, contents

**Envelope**:
The complete serializable GD-92 `<envelope>` data entity, containing routing and integrity information, exactly one set of Contents, and its Block Check Character.
_Avoid_: envelope header, Frame

**Envelope Reception**:
The validation of received encoded Envelope data into either a valid Envelope or a reported integrity failure.
_Avoid_: RabbitMQ handler, Frame decoding

**Contents**:
The type-specific user information carried within an Envelope. `IGD92MessageContents` is the public contract for encoded Contents.
_Avoid_: message, envelope, payload

**Unsupported Message Contents**:
The byte-preserved Contents of a known Message Type whose type-specific structure is not implemented by this User Agent.
_Avoid_: invalid Envelope, unknown Message Type

**Message Type**:
A numbered eight-bit Protocol Field, restricted to the finite catalogue defined by GD-92, that determines a Message's Contents structure and associated handling constraints.
_Avoid_: application event, arbitrary payload type

**Protocol Field**:
An encoded value type used to construct an Envelope or Message Contents.
_Avoid_: domain Entity, arbitrary JSON field

**Protocol Boolean**:
A Protocol Field whose encoded `00` and `01` values represent false and true. It exposes the semantic Boolean value, not its encoded octet.

**Word8**:
An unsigned eight-bit Protocol Field that is encoded as eight consecutive bits.
_Avoid_: signed byte, variable-length integer

**Block**:
The sequential number of one Text_message block.

**OfBlocks**:
The total number of blocks in one Text_message.

**Text**:
A long compressed ASCII Protocol Field used for unstructured message text, which is decompressed when received.

**Output Peripherals**:
A 16-bit Protocol Field whose set bits identify selected peripheral output functions or reported asserted output states.

**Input Peripherals**:
A 16-bit Protocol Field whose set bits report defined peripheral input states, including manual acknowledgement, power, repeat-message, battery, and paper conditions.

**Manual Acknowledgement Request**:
A boolean Protocol Field indicating that a recipient must await a local Manual Acknowledgement before completing a message.

**Alert Group**:
A two-character Protocol Field that selects one of the seven defined Firecall team combinations.

**Request Code**:
A one-character Protocol Field that identifies a request to speak, an emergency, or a confidential request to speak.

**Reset Type**:
A one-byte Protocol Field that identifies the requested reset severity: software reset, hardware reset, or hardware reset with permanent-parameter reload.

**Reset Reason**:
A one-byte Protocol Field that reports whether a reset was requested, caused by software failure, or followed power-on.

**Pager Priority**:
A one-character Protocol Field that classifies a pager call as emergency, priority, routine, or administrative.

**Pager Number**:
The Protocol Field that combines a Telephone Number with a Pager Type.

**Pager Type**:
A one-character Protocol Field that identifies an alphanumeric, numeric, or tones-only pager call.

**Pager Text**:
A compressed ASCII Protocol Field carrying up to 200 encoded bytes of pager text.

**Resource Status Fields**:
The Availability, location, status, and Remarks fields reported for a Resource. AVL Type and AVL Data remain opaque because Volume A reserves future AVL assignment values and does not define AVL Data coding.

**Resource Status**:
The Message Type 20 Contents comprising an ordered sequence of Resource Status entries, each reporting a Resource Callsign, AVL Type and Data, Status Code, and Remarks.

**Duty Staffing Update**:
The Message Type 21 Contents comprising an ordered sequence of Duty Staffing entries, each reporting a Resource Callsign, Officer in Charge, number of Riders, Status Code, and Remarks.

**Log Update**:
The Message Type 22 Contents that identifies the reporting Resource and Incident, and carries a compressed text update for that Incident's log.

**Stop**:
The Message Type 23 Contents by which a Resource identifies an Incident as stopped using a Stop Code, allowing control to prevent further deployment.

**Make-up**:
The Message Type 24 Contents by which a Resource requests up to 12 typed and quantified additional Appliances for an Incident.

**Incident Notification**:
The Message Type 31 Contents by which an alarm agency or another mobilising system reports an Incident's source, contact details, identifiers, address, and supporting text.

**Alert Status**:
The Message Type 42 Contents through which an Alerter reports a defined change in its status conditions.

**Alert Engineering**:
The Message Type 43 Contents through which a User Agent sends a defined engineering command to an Alerter.

**Test**:
The Message Type 64 Contents that requests a remote test through an opaque Test Type selected by the receiving Message Transfer Agent.

**Printer Status**:
The Message Type 65 Contents through which a printer User Agent reports its offline, paper-out, or online condition to the Network Management User Agent.

**MTA Status Change**:
The Message Type 66 Contents through which a Message Transfer Agent reports a defined change in its operational status.

**Route Status**:
The Message Type 67 Contents through which a Router enables or disables routes to Destination Nodes via the sending Communications Node.

**Brigade Message**:
The Type 101 non-mandatory Contents for non-operational text sent between Communications Nodes.

**Data Base Query**:
The Type 102 non-mandatory Contents carrying an opaque Query Type and text for a remote database query.

**Status Code**:
A one-byte Protocol Field that identifies one of Volume A's defined Resource availability and incident states.

**Officer In Charge**:
A bounded plain ASCII Protocol Field identifying a Resource's officer in charge.

**Riders**:
A one-byte Protocol Field recording the 1-15 riders reported with a Resource's Duty Staffing.

**Update**:
A compressed ASCII Protocol Field carrying up to 255 encoded bytes of incident-log update text.

**Stop Code**:
A fixed five-character alphanumeric Protocol Field supplied when a Resource stops an Incident.

**Appliance Type**:
A three-character, space-padded alphanumeric Protocol Field identifying an appliance type.

**Appliance Quantity**:
The unsigned one-byte quantity requested for an Appliance Type.

**Number Types**:
The unsigned one-byte count, limited to 0-12, of Appliance Type and Quantity pairs.

**Alarm Fields**:
The bounded Alarm Type, Call Agency, Alarm Reference, and Alarm Serial Protocol Fields that identify an Incident Notification source and alarm.

**Alerter Status**:
A two-character Protocol Field reporting a defined Alerter local operation, fault, or cleared-fault condition.

**Alerter Engineering**:
A one-character Protocol Field that selects a defined Alerter engineering command or user-defined engineering parameter.

**Destination Nodes**:
A count-prefixed collection of Address Ranges used to describe routing destinations.

**Format Type**:
A one-byte Protocol Field whose only currently defined value identifies a text table.

**Table**:
A compressed ASCII Protocol Field carrying up to 255 encoded bytes of formatted or proforma table data.

**Query Type**:
An opaque one-byte Protocol Field; Volume A leaves its values for later definition.

**Test Type**:
An opaque one-byte Protocol Field with no Volume A assignment table.

**Printer Status**:
A one-byte Protocol Field reporting a printer as offline, paper out, or online.

**MTA Status**:
A one-byte Protocol Field reporting a Message Transfer Agent as idle, online, user-offline, or fault-offline.

**Configuration Fields**:
The Protocol Fields that define Communications Node routing, bearer, peripheral-mapping, and addressing Parameters.

**Input Peripheral Map**:
A fixed three-byte Protocol Field that maps one input function to a Physical Bit, its Active State, and its Generate Alarm behavior.

**Output Peripheral Map**:
A fixed four-byte Protocol Field that maps one output function to a Physical Bit, its Active State, and its Pulse Length.

**Parameter Table Entry**:
A fixed Protocol Field record within a table-valued Parameter. The enclosing Parameter Value owns the boundary of an uncounted sequence of Parameter Table Entries.

**Table Value Field**:
An immutable uncounted collection of Parameter Table Entries representing one complete table-valued Parameter. It decodes entries until the supplied table-value buffer is exhausted.

**Scalar Parameter Fields**:
The named one- and two-octet Protocol Fields that represent router and Message Transfer Agent settings, timers, counters, and connection limits.

**MTA Minimum Message Priority**:
A Protocol Field that restricts an MTA to accepting Messages at or above a valid GD-92 Message Priority.

**Acknowledgement Character**:
The single 7-bit ASCII Protocol Field used as the acknowledgement character of an asynchronous MTA.

**Alternative Address Table Entry**:
A Protocol Field record pairing an Address Range with its compressed alternative printed address.

**Connection Table Entries**:
The typed LAN, telephone (PSTN and ISDN), Mobile Data Terminal, WAN, routing, and connection-statistics Protocol Field records used by table-valued Parameters.

**Active State**:
A one-byte Protocol Field that defines whether a physical peripheral contact is active when closed or open.

**Generate Alarm**:
A one-byte Protocol Field that defines whether an asserted or de-asserted input generates an alarm and, where applicable, repeats at its Regeneration Time.

**Agent Type**:
A one-byte Protocol Field that identifies the specified Message Transfer Agent or User Agent type, including Volume A's user-defined assignments.

**Connect Type**:
A one-byte Protocol Field identifying a permanent or switched virtual circuit.

**Dial Tones**:
A one-byte Protocol Field selecting pulse or tone dialling.

**Hold Time**:
A one-byte Protocol Field specifying the connection hold duration in seconds.

**Physical Bit**:
A one-byte Protocol Field identifying a physical peripheral bit from 0 through 15.

**Routing Preference**:
An opaque one-byte Protocol Field that orders routing entries; Volume A defines no value-assignment table.

**Configuration Address Fields**:
Bounded ASCII Protocol Fields for alternative printed addresses, LAN and WAN addresses, Network User addresses, and Communications Node names. Alternative printed addresses use compressed ASCII; the others use plain ASCII.

**Callsign List**:
A counted collection of the Callsigns of Resources to which a Mobilise Message applies.

**Time And Date**:
A fixed-length `DDMMMYYHHMMSS` Protocol Field recording a Message submission time.

**Mobilisation Type**:
A Protocol Field that classifies a mobilisation as a pre-alert, incident, non-incident, batch address, standby, demobilisation, or test.

**Incident Address**:
The structured address, location, and postcode information supplied for an Incident. Each typed address component owns its encoded-length and compression constraint.
_Avoid_: Communications Address, Address Range

**Text Message**:
The Message Type 27 Contents, composed of a Block, OfBlocks, and Text.
_Avoid_: text Protocol Field, Envelope

**Mobilise Command**:
The Message Type 1 Contents, composed of Output Peripherals and a Manual Acknowledgement Request.

**Page Officer**:
The Message Type 3 Contents that carries Pager Priority, Pager Number, and Pager Text.

**Area Page Message**:
The non-mandatory Message Type 4 Contents that carries Pager Priority, Pager Number, and Pager Text.

**Activate Peripheral**:
The Message Type 7 Contents that activates the selected Output Peripherals.

**Deactivate Peripheral**:
The Message Type 8 Contents that deactivates the selected Output Peripherals.

**Peripheral Status Request**:
The zero-content Message Type 9 request for the current state of a remote station's monitored peripherals.

**Peripheral Status**:
The Message Type 28 Contents that reports Input Peripherals followed by Output Peripherals.

**Alert Crew**:
The Message Type 40 Contents that alerts a Firecall team group, optionally requires Manual Acknowledgement, and can activate Output Peripherals.

**Interrupt Request**:
The Message Type 25 Contents through which a Resource signals a request to speak or an emergency.

**Reset Request**:
The Message Type 10 Contents that asks an addressed Router to perform a specified Reset Type.

**Reset**:
The Message Type 30 Contents through which a User Agent reports its Reset Reason.

**Mobilise Message**:
The Message Type 2 Contents, comprising message block data, a submission Time And Date, target Callsign List, and one or more incident-detail sets.

**Message Family**:
A protocol grouping of Message Types: Mobilisation, Resource or Incident, Peripheral, Protocol, Network Management, or Non-Mandatory.
_Avoid_: bounded context, transport layer

**Non-Mandatory Message Type**:
A Message Type that a User Agent may support as an optional extension to the core protocol catalogue.
_Avoid_: required capability, proprietary transport message

### Operational Content

**Incident**:
An operational occurrence identified in protocol messages by an Incident Number.
_Avoid_: Message, alarm

**Incident Number**:
The identifier for an Incident used in protocol messages.
_Avoid_: Message sequence number, alarm reference

**Incident Notification**:
An incoming alarm record that a receiving User Agent may accept into an Incident, defer, or reject.
_Avoid_: Incident, accepted mobilisation

**Resource**:
An operational firefighting resource identified in protocol messages by a Callsign.
_Avoid_: User Agent, Communications Node

**Resource Status**:
The protocol-visible availability, location, and remarks reported for a Resource.
_Avoid_: delivery status, User Agent status

**Resource Status Request**:
The Message Type 5 Contents that requests the current status of one or more Resources by Callsign. A single zero-length Callsign requests every Resource known to the recipient.

**Duty Staffing**:
The officer-in-charge, rider, and staffing details reported for a Resource.
_Avoid_: Resource Status, User Agent capability

**Incident Assignment**:
The protocol-visible relationship that identifies the Incident a Resource is currently attending.
_Avoid_: Communications Address, mobilisation workflow

**Callsign**:
The operational identifier of a Resource in protocol messages.
_Avoid_: Communications Address, Incident Number

**Peripheral**:
A logical input or output function of a Peripheral User Agent.
_Avoid_: physical channel, Peripheral User Agent

**Peripheral User Agent**:
A User Agent that remotely controls output Peripherals and monitors input Peripherals.
_Avoid_: Peripheral, physical I/O module

**Alerter User Agent**:
A User Agent that controls alerting and reports alerter status.
_Avoid_: paging service, hardware option

**Paging User Agent**:
A User Agent that sends officer pages through a public or private paging system.
_Avoid_: Alerter User Agent, paging bearer

**Printer User Agent**:
A User Agent that renders mobilisation and text Messages to a printer.
_Avoid_: printer device, display

**Resource User Agent**:
A User Agent that maintains and reports a table of Resources.
_Avoid_: Resource, resource database

### Addressing and Message Identity

**Communications Address**:
The unique three-part identifier of a port, consisting of a Brigade or Agency number, a Node number, and a Port number.
_Avoid_: CommsAddress, endpoint address

**Address Range**:
A pair of Communications Addresses that represents a range in routing or configuration data.
_Avoid_: Envelope destination

**Sequence Number**:
A value assigned by a Message originator to distinguish Messages sent to a destination.
_Avoid_: message ID, ordering number

**Message Originator**:
The Communications Entity or User Agent that creates a Message and assigns its source Communications Address and Sequence Number.
_Avoid_: UI controller, broker publisher

**Unique System-Wide Reference (USWR)**:
The unique identifier formed from one source Communications Address, one destination Communications Address, and one Sequence Number.
_Avoid_: multicast message ID, correlation ID

### Delivery Outcomes

**Message Transfer System (MTS)**:
The store-and-forward system formed by Communications Nodes that forwards Messages towards their destinations without an end-to-end connection.
_Avoid_: User Agent protocol, direct connection

**MTS Delivery Failure**:
A notification that the Message Transfer System could not deliver an acknowledgement-requested Message.
_Avoid_: delivery acknowledgement, successful delivery

**User-Agent Transaction Response**:
A response from a destination User Agent that reports a completed, rejected, or deferred action, or returns requested information.
_Avoid_: delivery acknowledgement, transport response

**Deferred Transaction Response**:
A User-Agent Transaction Response that confirms receipt and ongoing processing before a final response.
_Avoid_: final rejection, delivery failure

**Reason Code**:
A typed diagnostic, comprising a reason-code set and code, that explains a Negative Acknowledgement.
_Avoid_: free-text error, delivery acknowledgement

**General Reason Code**:
A Reason Code in the General reason-code set (set 1), such as Check Error, Invalid Protocol, or Invalid Message.
_Avoid_: arbitrary numeric error code, delivery acknowledgement

**Parameter Reason Code**:
A Reason Code in the Parameter reason-code set (set 4), such as Invalid Table or Invalid Parameter.
_Avoid_: General Reason Code, transport error

**Negative Acknowledgement (NAK)**:
A response Message from a Router or User Agent that reports delivery failure, rejected processing, or deferred processing.
_Avoid_: final failure, successful acknowledgement

**Acknowledgement (ACK)**:
A solicited response Message indicating that the original Message was received and processed.
_Avoid_: Message Transfer System delivery confirmation

**Manual Acknowledgement**:
A local action that completes pending manually acknowledged work at one Communications Node.
_Avoid_: Acknowledgement Message, delivery confirmation

**Acknowledgement Request**:
The Envelope flag indicating that a Message originator expects an MTS Delivery Failure or User-Agent Transaction Response.
_Avoid_: Message Type default, successful delivery acknowledgement

**Unacknowledged Delivery**:
A Message delivery, identified by a USWR, that awaits an MTS Delivery Failure or User-Agent Transaction Response.
_Avoid_: unacknowledged message, multicast state

### Routing

**Route Entry**:
A preferred path to a set of destination Communications Nodes through one adjacent Communications Node over a specified bearer type.
_Avoid_: port route, static MTA assignment

**MTA Availability**:
The current ability of a Message Transfer Agent to carry a Message for a next Communications Node at a given priority.
_Avoid_: route preference, destination reachability

**Node Reachability**:
The current ability to route a Message to a destination Communications Node through available Message Transfer Agents.
_Avoid_: Route Entry, static node property

**Forwarding Copy**:
A bearer-specific copy of a Message created by a Router for a non-overlapping subset of its destination addresses.
_Avoid_: retransmission, duplicate Message

## Relationships

- A **Communications Node** contains a router, zero or more User Agents, and zero or more Message Transfer Agents.
- An **Outstation** is a **Communications Node**.
- A **Communications Node** belongs to one **Brigade or Agency** through its **Communications Address**.
- A **Router** switches messages between the **User Agents** and **Message Transfer Agents** in its **Communications Node**.
- A **User Agent** submits each outgoing **Envelope** to its local **Router** through **Router Ingress**.
- A **Router** delivers an Envelope addressed to a local **User Agent** through **User-Agent Ingress**.
- A **Router** delivers a management **Envelope** addressed to a local **Message Transfer Agent** or **User Agent** through **Local Participant Ingress**.
- A **Router** performs **Local Delivery Classification** before delivering a locally addressed **Envelope**.
- **Local Delivery Classification** selects Router handling for a single Envelope addressed to the local Router, **Local Participant Ingress** for the established local Parameter management Message Types, **User-Agent Ingress** for other single local non-Router Envelopes, or an opaque not-locally-deliverable outcome.
- **Local Delivery Classification** does not select forwarding; non-local and multi-destination Envelopes are not locally deliverable.
- A **Router** delivers a single local non-Router **Parameter Request**, **Parameter Request Multiple**, or **Set Parameter** through **Local Participant Ingress**.
- A **Router** delivers a locally addressed **Parameter Message**, **Acknowledgement**, or **Negative Acknowledgement** through **User-Agent Ingress**.
- A **Message Transfer Agent** transfers messages only with its paired **Message Transfer Agent**.
- A **Message Transfer Agent** transfers an encoded **Envelope** to its paired Message Transfer Agent in a **Frame**.
- A **Router** and **User Agent** validate the **Block Check Character** of a received **Envelope**.
- A Router or **User Agent** attempts a **Negative Acknowledgement** with a **General Reason Code** when a received Envelope's length, **Block Check Character**, or **Protocol Version** is invalid.
- A **User Agent** has exactly one **Communications Address** and one or more **UA Capabilities**.
- A **Network Management User Agent** is a **User Agent** that fulfils a **Network Manager** or **Alternative Network Manager** role.
- An **Alerter User Agent**, **Paging User Agent**, **Printer User Agent**, **Peripheral User Agent**, or **Resource User Agent** is a **User Agent** with the corresponding **UA Capabilities**.
- Every **User Agent** supports the **Network Manager** and **Alternative Network Manager** addresses.
- An **External System** may present one or more addressed **User Agents** through a combined **Message Transfer Agent** and User Agent interface.
- An **External Bearer** carries **Messages** but has no **Communications Address**.
- A **Router**, **Message Transfer Agent**, or **User Agent** owns zero or more **Parameters**.
- A **Parameter Table** belongs to exactly one **Router**, **Message Transfer Agent**, or **User Agent** port.
- A **Participant Parameter Store** persists only its owning participant's Permanent and Non-Volatile **Parameter Tables**; physical database infrastructure may be shared without sharing Parameter ownership.
- The first initialization of a **Participant Parameter Store** commits all bootstrap **Parameter** Values as one set.
- A **Communications Node** has one **Node Parameter Set** maintained through its **Router**.
- A **Communications Node Inventory** identifies the Router, zero or more **Message Transfer Agents**, and zero or more **User Agents** at one **Communications Node**.
- A **Message Transfer Agent** or **User Agent** has one **Agent Type**.
- An **Unclassified Participant** has a user-defined **Agent Type**.
- A **Communications Node Inventory** is produced by one **Inventory Scan** and includes its **Inventory Scan Summary**.
- A **Current Parameter Table** is initialized from its **Non-Volatile Parameter Table** on normal startup.
- A **Permanent Parameter Table** supplies fallback Parameter values when a **Non-Volatile Parameter Table** is corrupted.
- A Router dispatches locally addressed, single-destination Parameter management Envelopes to its **Router Parameter Read**, **Node Login**, or **Level 1 Password Modification** Module.
- A **Router Parameter Read** returns only a Router-owned Current Parameter Value; unsupported Router Parameters remain unhandled by the Router.
- A **Node Login** Module owns Current Password logon and logoff behavior and does not persist Node Login state.
- A **Level 1 Password Modification** acknowledges a Non-Volatile change only after its Router-owned password verifier is durably stored.
- Every **Management Transaction** uses the NodeManager's configured retry policy and retains a terminal timeout outcome when its attempts are exhausted.
- A **Management Transaction** owns its response correlation and exposes its status or terminal outcome without exposing its correlation records.
- A Router Ingress submission failure is a terminal **Management Transaction** delivery failure.
- **Management Transaction** state is volatile and is discarded when NodeManager restarts.
- A Router-owned **Node Login** at a **Communications Node** authorizes Parameter modification according to each **Parameter**'s **Password Level**.
- A Router has at most one active **Node Login**; a successful login from another User-Agent Communications Address replaces it.
- A failed attempt to establish a **Node Login** leaves the existing Node Login unchanged.
- A **Node Login** remains active until it is replaced or logged off.
- Any User-Agent Communications Address can clear the active **Node Login** by setting the **Current Password** to Password Level 0.
- Changing the **Level 1 Password** does not alter the active **Node Login**.
- A changed Non-Volatile **Level 1 Password** takes effect when it is copied to the Current Parameter Table on startup or reset.
- A read of a **Current Password** or **Level 1 Password** redacts its Password as the string `PASSWORD`.
- A **Parameter Request** identifies one **Parameter Table** and one **Parameter Number**.
- A **Parameter Number** identifies one **Parameter** within its **Parameter Table**.
- A **Parameter Request** or **Set Parameter** addressed to a **Message Transfer Agent** or **User Agent** is delivered to that participant; the participant validates and responds for its own **Parameters**.
- A **Message Originator** normally sets an **Acknowledgement Request** on a **Parameter Request Envelope**, while the recipient always returns a **Parameter Message** or **Negative Acknowledgement**.
- A **Parameter Request Multiple** identifies one **Parameter Table**, one **Parameter Number**, and one **Parameter Entry Selection**.
- A **Parameter Request Multiple Envelope** always has its **Acknowledgement Request** set.
- A **Parameter Entry Selection** is either a nonzero inclusive **Parameter Entry Index** range, or a request for the most recent entries whose encoded first index is zero and whose encoded last index is a **Parameter Entry Count**.
- A **Parameter Message** contains one **Parameter Value** and one **More Values** field.
- A **Parameter Message Envelope** responds to a **Parameter Request Envelope** using the request Envelope's source, Message Priority, and Sequence Number, with its **Acknowledgement Request** clear.
- A **User Agent** accepts **Messages** at its current **Protocol Version** or below.
- An **Envelope** carries the **Protocol Version** applicable to its **Message Type**.
- A **Message Sequence** contains one or more **Messages**, each with a distinct **Sequence Number**.
- A **Router** processes higher **Message Priorities** before lower ones and preserves arrival order within the same priority.
- A **Message** is represented by an **Envelope** containing exactly one set of **Contents**.
- An **Envelope** and **Contents** are composed of **Protocol Fields**.
- A Router or **User Agent** performs **Envelope Reception** before processing a received **Envelope**.
- An **Ingress Envelope Transport** rejects an encoded Envelope with trailing bytes using one transport-wide validation error.
- User-Agent Ingress and Local Participant Ingress require exactly one Envelope destination before deriving their address-specific RabbitMQ endpoint.
- Router Ingress submits to its configured local Router endpoint; Router handling validates the submitted Envelope destination.
- An **Envelope** with a known but unimplemented **Message Type** preserves its **Unsupported Message Contents** without interpreting them.
- A Router or **User Agent** responds to an acknowledgement-requested, unsupported **Message Type** with a **Negative Acknowledgement** using the General Reason Code `inv_mess`; otherwise it discards the Envelope.
- An **Envelope** derives its CountAndLength, Message Type, and Block Check Character from its Destinations and Message Contents.
- An **Envelope** validates its received Block Check Character against all preceding encoded Envelope and Contents bytes.
- A **Message Type** determines the structure and handling constraints of a Message's **Contents**.
- A **Text Message** contains one **Block**, one **OfBlocks**, and one **Text**.
- An **Envelope** contains between 1 and 63 unique destination **Communications Addresses**.
- **Destinations** preserves its Communications Addresses in their declared order.
- Each **Message Type** belongs to one **Message Family**.
- A **Non-Mandatory Message Type** is usable only when the relevant **User Agents** have that **UA Capability**.
- An **Incident Number** identifies one **Incident**.
- An **Incident Notification** may be accepted into one **Incident**.
- A **Callsign** identifies a **Resource** in a protocol message.
- A **Resource** has zero or more reported **Resource Statuses** over time.
- A **Resource** has zero or more reported **Duty Staffing** updates over time.
- A **Resource** has zero or one current **Incident Assignment**.
- An **Incident** has zero or more **Incident Assignments**.
- A **Peripheral User Agent** exposes zero or more **Peripherals** whose physical mappings are held in **Parameters**.
- A **User Agent** interprets **Contents**, while a **Communications Entity** transfers a **Message** by its **Envelope**.
- A **Communications Address** identifies exactly one port within a **Communications Node**.
- An **Address Range** identifies a set of destination Communications Nodes for routing or configuration.
- A **Router** has a **Communications Address** whose Port number is 0.
- A Router's current **Parameter Table** exposes its **Brigade or Agency** identifier as Parameter Number 1.
- A **Message Originator** assigns an outgoing **Envelope** its source **Communications Address** and **Sequence Number**.
- A **Unique System-Wide Reference** identifies one source-to-destination **Message** using its **Sequence Number**.
- The **Message Transfer System** forwards a **Message** between **Communications Nodes** without interpreting its **Contents**.
- A **Message** with an **Acknowledgement Request** results in either an **MTS Delivery Failure** or a **User-Agent Transaction Response**.
- The **Message Transfer System** sends no positive response for successful Message delivery.
- An **Unacknowledged Delivery** is retried with the same **Envelope** after timeout and is resolved by an **MTS Delivery Failure** or final **User-Agent Transaction Response**.
- A **Deferred Transaction Response** keeps its **Unacknowledged Delivery** pending until a final **User-Agent Transaction Response**.
- A rejected **User-Agent Transaction Response** includes a **Reason Code**.
- A **Negative Acknowledgement** identifies one or more affected destination **Communications Addresses** and a **Reason Code**.
- An ordinary **Negative Acknowledgement** is a terminal **Management Transaction** outcome; `wait_ack` is a **Deferred Transaction Response** that remains pending for a later final outcome.
- A `wait_ack` **Deferred Transaction Response** stops **Management Transaction** retransmission and starts one final-response timeout.
- A **General Reason Code** is a **Reason Code** from reason-code set 1.
- A **Parameter Reason Code** is a **Reason Code** from reason-code set 4.
- An **Acknowledgement** is a final **User-Agent Transaction Response** for one destination.
- A **Negative Acknowledgement Envelope** responding to an acknowledgement-requested **Envelope** targets the received Envelope's source, preserves its Message Priority and Sequence Number, and does not request an acknowledgement.
- A **Manual Acknowledgement** causes the relevant local **User Agent** to send a final **Acknowledgement**.
- A **Router** selects a **Route Entry** by destination Communications Node, Message priority, and **MTA Availability**.
- A **Route Entry** identifies one adjacent **Communications Node**, a bearer type, and an ordered preference.
- A **Router** derives **Node Reachability** from its **Route Entries** and **MTA Availability** and announces changes to adjacent Communications Nodes.
- A **Router** creates one **Forwarding Copy** for each bearer required by a multi-destination **Message**.
- Each destination Communications Address appears in exactly one **Forwarding Copy** for a routing attempt.

## Flagged ambiguities

- The existing feature fixture assigns the Router port 25 — resolved: a **Router** is always addressed at Port number 0.
- The existing MessageTypes enum assigns ACK number 51 and NAK number 52 — resolved: **Message Type** 50 is ACK and 51 is NAK.
- The existing JSON schema models destination address ranges — resolved: an **Envelope** contains between 1 and 63 unique destination **Communications Addresses**.
- The specification's term "data entity" can be mistaken for an identity-bearing model object — resolved: it is a **Protocol Field** value type.
- A NAK does not always reject a transaction — resolved: `wait_ack` is a **Deferred Transaction Response** that requires a later final response.
- The specification sometimes calls a Callsign's owner a "user" — resolved: a **Callsign** identifies the acting **Resource**; no separate User concept is defined.
- Mobilisation can imply a business workflow across messages — resolved: Volume A defines no correlation between Mobilise_command and Mobilise_message, so they remain independent **Message Types**.
- A browser login can be confused with a **Node Login** — resolved: browser credentials are not a GD-92 concept, and read-only Parameter Requests do not require a **Node Login**.

## Example dialogue

> **Dev:** "Is a fire station fundamentally different from a control-room subsystem in the communications model?"
> **Domain expert:** "No — each is a peer **Communications Node**, regardless of the equipment it represents."
