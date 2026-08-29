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

**Communications Entity (CE)**:
A Router or Message Transfer Agent that transfers messages across the communications network without interpreting ordinary message contents.
_Avoid_: user agent, endpoint

**Router**:
A Communications Entity that orderly switches messages between User Agents and Message Transfer Agents within a Communications Node.
_Avoid_: communications node, Message Transfer Agent

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

### Configuration

**Parameter**:
A protocol-defined configuration value owned by a Router, Message Transfer Agent, or User Agent port.
_Avoid_: application setting, global configuration

**Password Level**:
The access level required to modify a Parameter.
_Avoid_: user role, permanent authentication

**Node Login**:
The temporary authenticated state that authorizes Parameter modification at one Communications Node.
_Avoid_: system-wide login, device session

### Message Structure and Classification

**Protocol Version**:
The version of the GD-92 protocol used to encode a Message.
_Avoid_: node deployment version, software version

**ProtocolAndPriority**:
The packed Envelope field containing a Message Priority and Protocol Version.

**Frame**:
The Message Transfer Agent-to-Message Transfer Agent transfer unit containing a Message and bearer-protocol overhead.
_Avoid_: Message, Contents

**Block Check Character (BCC)**:
The value calculated over an Envelope to verify the end-to-end integrity of its Message.
_Avoid_: frame checksum, bearer validation

**Message Sequence**:
An ordered set of independently sequenced Messages that conveys content too long for one protocol Message.
_Avoid_: frame fragmentation, oversized Message

**Message Priority**:
The ordered delivery urgency of a Message, ranging from 1 (highest) to 9 (lowest).
_Avoid_: queue hint, business severity

**Message**:
The basic unit of network communication composed of an Envelope and Contents.
_Avoid_: envelope, contents

**Envelope**:
The Message part containing information used solely to transfer that Message across the network.
_Avoid_: message, contents

**Contents**:
The Message part containing user information.
_Avoid_: message, envelope, payload

**Message Type**:
A numbered protocol classification that determines a Message's Contents structure and associated handling constraints.
_Avoid_: application event, arbitrary payload type

**Protocol Field**:
An encoded value type used to construct an Envelope or Message Contents.
_Avoid_: domain Entity, arbitrary JSON field

**Word8**:
An unsigned eight-bit Protocol Field that is encoded as eight consecutive bits.
_Avoid_: signed byte, variable-length integer

**Block**:
The sequential number of one Text_message block.

**OfBlocks**:
The total number of blocks in one Text_message.

**Text**:
A long compressed ASCII Protocol Field used for unstructured message text.

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
- A **Message Transfer Agent** transfers messages only with its paired **Message Transfer Agent**.
- A **Message Transfer Agent** transfers a **Message** to its paired Message Transfer Agent in a **Frame**.
- A **Router** and **User Agent** validate the **Block Check Character** of a received **Message**.
- A **User Agent** has exactly one **Communications Address** and one or more **UA Capabilities**.
- An **Alerter User Agent**, **Paging User Agent**, **Printer User Agent**, **Peripheral User Agent**, or **Resource User Agent** is a **User Agent** with the corresponding **UA Capabilities**.
- Every **User Agent** supports the **Network Manager** and **Alternative Network Manager** addresses.
- An **External System** may present one or more addressed **User Agents** through a combined **Message Transfer Agent** and User Agent interface.
- An **External Bearer** carries **Messages** but has no **Communications Address**.
- A **Router**, **Message Transfer Agent**, or **User Agent** owns zero or more **Parameters**.
- A **Node Login** at a **Communications Node** authorizes Parameter modification according to each **Parameter**'s **Password Level**.
- A **User Agent** accepts **Messages** at its current **Protocol Version** or below.
- A **Message** carries the **Protocol Version** applicable to its **Message Type**.
- A **Message Sequence** contains one or more **Messages**, each with a distinct **Sequence Number**.
- A **Router** processes higher **Message Priorities** before lower ones and preserves arrival order within the same priority.
- A **Message** contains exactly one **Envelope** and exactly one set of **Contents**.
- An **Envelope** and **Contents** are composed of **Protocol Fields**.
- A **Message Type** determines the structure and handling constraints of a Message's **Contents**.
- An **Envelope** contains between 1 and 63 unique destination **Communications Addresses**.
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
- A **Unique System-Wide Reference** identifies one source-to-destination **Message** using its **Sequence Number**.
- The **Message Transfer System** forwards a **Message** between **Communications Nodes** without interpreting its **Contents**.
- A **Message** with an **Acknowledgement Request** results in either an **MTS Delivery Failure** or a **User-Agent Transaction Response**.
- The **Message Transfer System** sends no positive response for successful Message delivery.
- An **Unacknowledged Delivery** is retried with the same **Envelope** after timeout and is resolved by an **MTS Delivery Failure** or final **User-Agent Transaction Response**.
- A **Deferred Transaction Response** keeps its **Unacknowledged Delivery** pending until a final **User-Agent Transaction Response**.
- A rejected **User-Agent Transaction Response** includes a **Reason Code**.
- A **Negative Acknowledgement** identifies one or more affected destination **Communications Addresses** and a **Reason Code**.
- An **Acknowledgement** is a final **User-Agent Transaction Response** for one destination.
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

## Example dialogue

> **Dev:** "Is a fire station fundamentally different from a control-room subsystem in the communications model?"
> **Domain expert:** "No — each is a peer **Communications Node**, regardless of the equipment it represents."
