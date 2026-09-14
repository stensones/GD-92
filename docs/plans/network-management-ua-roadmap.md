# Network Management UA Roadmap

## Purpose

Evolve the Node Manager from a local Router-management prototype into the GD-92
Network Management UA. The implementation must communicate exclusively through
standard GD-92 messages and must not read or change another participant's
database directly.

## Authoritative sources

- GD-92 Volume A, section 6.2: common User Agent message handling and
  acknowledgement behavior.
- GD-92 Volume B, sections 4.1-4.7: Network Management Terminal / Network
  Management UA requirements.
- `docs\adr\0001-gd92-volume-a-is-the-protocol-source-of-truth.md`: Volume A
  is authoritative for interoperable protocol semantics. Volume B supplies the
  management-terminal product requirements recorded by this roadmap.
- `docs\plans\participant-parameter-browsing.md`: detailed existing plan for
  participant Parameter browsing.

## Current baseline

The Node Manager currently:

- is a Network Management UA at a configured local address;
- communicates with its configured local Router through RabbitMQ ingress;
- discovers ports 1-63 on its local Communications Node by reading Current
  Parameter 2 (`agent_type`);
- lets an operator select a Parameter Table before requesting a modeled scalar
  Parameter; LAN MTA supports its Current, Non-Volatile, and Permanent scalar
  reads through this path;
- reads and presents Router Current Parameters 2 (`node_number`) and 3
  (`node_name`) as typed GD-92 Node Number and Node Name values;
- reads and presents Router Current Parameter 9 (`maximum_message_length`) as
  its typed GD-92 Maximum Message Length value;
- reads and presents Router Current Parameter 10 (`network_manager_address_1`)
  as its typed GD-92 primary Network Manager address;
- reads and presents Router Current Parameter 11 (`network_manager_address_2`)
  as its typed GD-92 alternative Network Manager address;
- reads and presents Router Current Parameter 18
  (`manual_acknowledgement_timeout`) as its typed GD-92 manual acknowledgement
  timeout;
- reads and presents Router Current Parameter 20 (`time_and_date`) from the
  Router's live UTC clock;
- completes the discovered Router's supported scalar Current Parameter
  catalogue as concurrent, independently correlated requests; Password
  Parameters 5-8 are redacted consistently as `PASSWORD`;
- supports paged Current Router Routing Table reads through `Param_req_multiple`
  for requested entry ranges, returning decoded entry index, next-node values,
  and `more_values` through an operator-facing Parameters form when the
  1,023-byte GD-92 Parameter message-content limit truncates a response; **Next
  entries** requests from the last returned index through the original range
  upper bound and is unavailable on the final page;
- supports selected Current Router PSTN Table entry ranges through
  `Param_req_multiple`, returning decoded telephone-routing fields;
- supports selected Current Router WAN Table entry ranges through
  `Param_req_multiple`, returning decoded WAN-routing fields;
- requests Router Parameter values and performs Router logon/logoff;
- correlates expected `Parameter`, `ACK`, and `NAK` responses with retry and
  timeout status for operator polling;
- serves its own Current Parameters 1-3.

The current Parameter browser uses static presentation catalogues keyed by
Agent Type. It is intentionally not protocol catalogue discovery: GD-92 does
not define such a message.

## Delivery sequence

### 1. Complete general Parameter maintenance

GD-92 Volume B 4.2.3-4.2.6 requires parameter reads and changes, selectable
Parameter Tables, complete views, extensibility, and optionally multi-node
changes.

1. Allow the operator to select `Permanent`, `Non-Volatile`, or `Current`
   before requesting a Parameter.
2. Router Routing Table Current-table entry ranges now support
   `Param_req_multiple`, capacity-limited `more_values`, and browser paging.
   Extend these semantics to remaining table-shaped Parameters only through
   separately approved workflows.
3. Router Current Routing Table reads reject a range containing a missing entry
   with `NAK(parameter / inv_entry)`, and Router Current scalar reads reject
   an undefined Parameter Number with `NAK(parameter / inv_param)`. Extend
   parameter `NAK`s to every participant for invalid Parameter Tables, fields,
   and entries.
4. Add authorised generic `Set_parameter` workflows. Preserve
   participant-owned validation, persistence, and password/access rules.
5. Add a configurable catalogue/provider model for new Agent Types and
   Parameter formatting, without implying remote catalogue discovery.
6. Add selected multi-node Parameter-change jobs, with per-destination status,
   only after a correct one-participant change flow exists.

**Acceptance outcome:** An operator can read or authorisedly change a known
Parameter Table through GD-92 messages, including paged table values, and can
understand ACK, NAK, timeout, and delivery-failure outcomes.

### 2. Make common UA protocol behavior complete

GD-92 Volume A 6.2 requires every UA to recognise `ACK`, `NAK`,
`set_parameter`, `parameter_request`, and `param_req_multiple`; it also defines
invalid-message, unsupported-protocol, `check_error`, and `wait_ack` behavior.

1. Add a protocol dispatch boundary that distinguishes management commands,
   responses, and unsolicited notifications.
2. Implement valid inbound `Set_parameter` and `Param_req_multiple` behavior
   for Node Manager-owned Parameters, including modification of common
   Parameters 1 and 2 where permitted.
3. Send `general / inv_mess` NAKs for unrecognised acknowledged messages;
   discard unacknowledged unknown messages.
4. Reject a higher Protocol Version with `general / inv_prot`.
5. Model `check_error` retransmission, `wait_ack` deferred completion, the
   relevant timers, and an operator-visible failure when a deferred response
   never completes.
6. Implement primary/alternative network-manager behavior for delivery NAKs.

**Acceptance outcome:** Node Manager acts as a standards-complete UA rather
than only a client for its own Parameter transactions.

### 3. Build durable alarm management

GD-92 Volume B 4.2.7-4.2.11 requires alarm presentation, audible severity
notification, acceptance/deletion/clearing, and a lifecycle log.

1. Define a persistent alarm domain model: source address, message kind,
   severity, received time, payload/detail, accepted time/operator, cleared
   time, and deleted state.
2. Persist and display unsolicited failure/status messages as alarms.
3. Add acknowledgement, deletion, and condition-cleared workflows.
4. Provide audible and visible notification that continues until accepted,
   with a user-configurable accessibility-safe implementation.
5. Retain a queryable lifecycle/audit log across process restarts.

The initial inbound message set must include:

- `printer_status`;
- `MTA_status_change`;
- `alert_status`;
- `reset`;
- `peripheral_status`;
- `text_message`;
- unsolicited `parameter`, `ACK`, and `NAK` messages where they indicate a
  management condition.

**Acceptance outcome:** A fault notification is retained with time and source,
is visible to an operator, raises an appropriate alert, and has a durable
accepted/cleared/deleted history.

### 4. Add on-demand operational commands and tests

GD-92 Volume B 4.2.12-4.2.15 and 4.5 require commands to test and manage nodes
and bearers.

1. Provide operator workflows for `activate_peripheral`,
   `deactivate_peripheral`, `peripheral_status_request`, `reset_request`,
   `alert_crew`, `alert_eng`, and `test`.
2. Present acknowledgement, rejection, deferred completion, timeout, and
   resulting status to the operator.
3. Allow an operator to select the destination address and supported test
   type, then present its result.
4. Add a durable test schedule with address, test type, interval (one minute
   to 24 hours), last-run time, result, and failure alarm policy.

**Acceptance outcome:** An operator can invoke a standard GD-92 management
command, see its full result, and configure an automatic periodic test whose
failures become alarms.

### 5. Collect statistics and establish thresholds

GD-92 Volume B 4.2.16-4.2.17 requires PSTN/ISDN and MTA performance statistics,
reports, and alarms for abnormal failure levels.

1. Identify the relevant Parameter and status sources for each supported MTA
   and bearer type.
2. Collect timestamped snapshots through standard Parameter requests.
3. Store and report trends, failures, and aggregation periods.
4. Configure thresholds and generate alarms when failure rates are abnormal.

**Acceptance outcome:** Operators can view communications performance reports
and receive alarms from defined failure-rate thresholds.

### 6. Extend from local-node management to network management

GD-92 Volume B 4.1 and 4.7 require management of the communications network,
with a minimum capacity of 300 nodes.

1. Add an explicit remote-node/address selection model and network topology
   view.
2. Support management through the configured communications topology, rather
   than limiting discovery to ports on the local Router's node.
3. Define supported LAN, asynchronous, and WAN MTA configurations and their
   constraints as required by Volume B 4.6.
4. Test the supported capacity, including command progress, transaction
   isolation, and alarm/telemetry volume.

**Acceptance outcome:** The product can manage its declared network scope,
with documented MTA topology support and demonstrated capacity.

### 7. Meet operator experience and resilience requirements

GD-92 Volume B 4.3 and 4.7 require an efficient interface, progress within two
seconds, command abort while waiting, and a management-terminal failure that
does not impair other system elements.

1. Make every long-running command show progress within two seconds.
2. Add an explicit operator abort flow that safely releases the transaction
   and explains that a remote response may still arrive.
3. Persist active work, alarms, schedules, and reports so restart recovery is
   deliberate rather than accidental.
4. Define availability, backup, restore, monitoring, and failure-isolation
   measures for the stated availability target.
5. Keep the infrequent-operator alarm view simple while allowing the
   communications manager to access frequent commands quickly.

**Acceptance outcome:** Long-running operations are controllable and
recoverable, management state survives restart, and a Node Manager failure does
not impair message transfer by other participants.

## Cross-cutting design rules

- Node Manager is a normal GD-92 UA. It must use standard messages, even when
  it manages locally hosted participants.
- Participant applications retain ownership of their Parameter schemas and
  stores. Node Manager owns presentation metadata, transactions, alarms, test
  schedules, and reports.
- Do not expose passwords in browser responses, logs, persistence, or status
  text.
- Model acknowledgement requirements, sequence-number correlation, priority,
  and reason-code semantics at the protocol boundary rather than in UI code.
- Make long-running operations durable and observable; do not use an
  in-memory dictionary as the source of truth for operator-visible history.
- Deliver vertical slices through executable behavior tests: one protocol
  scenario, one management outcome, then the smallest supporting implementation.

## Priority

The recommended order is:

1. General Parameter maintenance and complete common-UA semantics.
2. Durable alarm handling and the required inbound management messages.
3. On-demand commands and test management.
4. Statistics and threshold alarms.
5. Remote/network-scale management, MTA topology, and resilience work.
