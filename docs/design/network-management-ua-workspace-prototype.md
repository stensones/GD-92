# Network Management UA Workspace Prototype

## Status

Approved design reference for the Node Manager UI.

This document extends `docs\design\node-manager-ui-prototype.md`. The existing
prototype remains the source for the visual language and application-shell
layout. This document defines the information architecture and behavior needed
as Node Manager becomes the GD-92 Network Management UA described in
`docs\plans\network-management-ua-roadmap.md`.

## Design intent

Node Manager is an operational console for a GD-92 Network Management UA, not
a general-purpose dashboard. It must let an operator:

- identify the current station, communications address, and operating state;
- select a network address and safely perform a management action;
- understand acknowledgement, rejection, timeout, and delivery results;
- recognise, accept, investigate, and clear alarm conditions;
- inspect the operational history without exposing credentials or masking
  protocol semantics.

The UI must not present a control as executable before the corresponding
GD-92 capability is implemented. It may show a future capability as
unavailable, with a clear explanation, but it must never simulate success.

## Application shell

Keep the current viewport grid:

1. Persistent GD-92 application header.
2. Main application shell, with navigation and independently scrollable
   workspace content.
3. Persistent footer.

The header continues to identify the product, Station End, local station
address, and connection state. The current visual language remains:

- navy for product identity;
- teal for normal/executable operational state;
- amber for attention/context;
- neutral surfaces for data;
- text labels, icons, and colour together for every state;
- visible keyboard focus, a skip link, semantic headings, and responsive
  layouts.

## Persistent workspace context

Every management workspace begins beneath its title with a context bar:

| Context element | Meaning |
|---|---|
| Selected Communications Address | The address to which an address-scoped action applies |
| Connection / last-seen state | The best-known communications state and its observation time |
| Active command count | Number of outstanding management transactions for the selected address |
| Activity control | Opens the command-status tray for all active and recent transactions |

Selecting an address in Network, an alarm, a command result, or a statistic
updates this shared context. The heading and URL must identify the selected
address, avoiding ambiguous actions and allowing an operator to revisit a
specific view.

## Activity tray

The Activity tray is the single operator-facing representation of a GD-92
management transaction. It replaces isolated polling messages without hiding
their protocol outcomes.

Each activity item displays:

- destination Communications Address;
- requested message or operator action;
- start time and elapsed time;
- progress state;
- response: ACK, NAK with decoded reason code, returned value/status, timeout,
  delivery failure, or local cancellation;
- a detail view containing the affected Parameter, command fields, and
  follow-up result where relevant.

While the matching protocol capability exists, an outstanding activity offers
**Abort**. Aborting stops Node Manager's local wait/retry behavior and records
the local cancellation; it must clearly explain that a remote response can
still arrive.

Commands must show a visible progress state within two seconds. A status cannot
be inferred from the disappearance of a button or row.

## Navigation

Navigation is task-oriented. It is compact on wide screens and becomes an
accessible menu on narrow screens.

| Group | Workspace | Primary responsibility |
|---|---|---|
| Monitor | Overview | Triage alarms, active work, recent failures, and communications health |
| Monitor | Network | Browse topology, select nodes/participants, and discover local participants |
| Manage | Parameters | Read, page, compare, and authorisedly change Parameters |
| Manage | Commands | Send supported peripheral, alerting, reset, and test commands |
| Monitor | Alarms | Work the durable alarm queue and investigate lifecycle history |
| Operate | Tests | Run an on-demand test and manage scheduled tests |
| Analyse | Statistics | Review MTA/bearer reports and threshold breaches |
| Administration | Router access | Perform Router logon/logoff and manage authorised access |
| Administration | Connectivity | Configure manager identity and supported MTA topology |

Navigation only exposes a workspace when its basic presentation has value. A
workspace without an implemented backend capability must communicate its
unavailability plainly and must not contain a control that appears to send a
message.

## Workspace designs

### Overview

Overview is an operator triage page rather than a generic product dashboard.
It contains:

- an ordered active-alarm summary, with unaccepted alarms first;
- active and recently completed command activity;
- test failures and due/overdue scheduled tests;
- communications health by MTA/bearer;
- recently changed Parameters.

Each summary item links to its relevant detail workspace with the address and
time context preserved. A quiet station uses an explicit normal-state message;
it does not present blank panels as success.

### Network

Network combines topology browsing and the existing local Participant
discovery flow.

The primary view is a searchable node/participant table or hierarchy showing:

- node and port address;
- Agent Type;
- health/last-seen state;
- active alarm count;
- supported management capabilities.

The existing discovery action remains available when the selected node is
local. It requests Agent Type through GD-92 Parameter messages; the UI must
not imply that it can discover remote catalogues until a remote discovery
protocol exists.

Selecting a participant opens an address-specific detail workspace with
summary, Parameters, Alarms, Commands, Test history, and Statistics tabs.
Tabs absent for an unsupported Agent Type show an explanation, not an empty
surface.

### Parameters

Parameters is the first roadmap workspace to deliver fully.

The view contains:

1. Selected address and Agent Type.
2. A Parameter Table selector: `Permanent`, `Non-Volatile`, or `Current`.
3. A searchable static/configurable catalogue appropriate to the Agent Type.
   It is presentation metadata, not a claim of remote GD-92 catalogue
   discovery.
4. A typed value table with Number, Name, Value, Read State, and Last Read.
5. A detail panel for a selected scalar Parameter or table-shaped Parameter.
6. A paged table-value browser driven by `Param_req_multiple` and
   `more_values`.

Normal reads send a single standard request and write their result into the
corresponding row. Pending, returned, rejected, timed-out, and delivery-failed
states remain visible in both the row and Activity tray.

**Edit mode** is separate from reading. It shows:

- the prior value;
- a constrained new-value editor;
- Parameter Table and destination;
- access/password requirements without exposing password data;
- an explicit review-and-send action;
- the resulting per-destination ACK or NAK.

Multi-node changes require an address-target review step. The UI displays every
target and its independent result; it never represents partial success as a
single successful change. Password values are permanently redacted in tables,
detail panels, Activity, logs, and audit history.

### Alarms

Alarms is a durable, severity-ordered work queue. Its list columns are:

- severity and current lifecycle state;
- received time;
- source Communications Address;
- message kind;
- concise condition;
- accepted/cleared time where applicable.

The detail pane contains the decoded GD-92 message detail, source context,
related statistics/commands, and lifecycle history. It supports:

- **Accept**: stops the audible alert and records operator/time;
- **Delete**: hides an operator-dismissed record while retaining its audit
  history according to policy;
- **Clear condition**: records a matching clear notification or an explicitly
  confirmed operator action where protocol semantics allow it.

Unaccepted alarms use both a persistent visual indicator and an audible
notification appropriate to their severity. The audible indication continues
until acceptance; accessibility and local notification settings must be
respected.

### Commands

Commands is address-scoped. It presents a capability-filtered catalogue grouped
as:

- Peripheral: activate/deactivate and request peripheral status;
- Alerting: alert crew and alert engineering;
- Reset;
- Test.

Each command opens a constrained form with GD-92 terminology and legal values,
then a review panel. The review panel shows destination, encoded intent,
priority/acknowledgement implications, and the expected result before sending.
After submission, the outcome appears in the command panel and Activity tray.

Commands not yet supported by the target participant or backend appear as
unavailable with a capability reason. They do not offer a submit control.

### Tests

Tests supports two modes:

- **Run now**: operator selects a destination address and test type, submits
  the GD-92 test request, and sees the result in Activity and the test history.
- **Schedule**: operator defines address, test type, interval from one minute
  to 24 hours, last run, next run, latest result, and failure-alarm policy.

Scheduled test failures create or update an alarm. The detail view links a
failure to the relevant alarm and shows when the selected test last ran.

### Statistics

Statistics presents MTA and bearer performance, initially as timestamped
Parameter/status snapshots and later as reports. It provides:

- metric and source selection;
- time-range selection;
- trend and aggregate views;
- configured threshold markers;
- direct links from a threshold breach to its resulting alarm.

An unavailable MTA/bearer metric is stated explicitly rather than plotted as
zero or healthy.

### Administration

**Router access** retains the existing Router logon/logoff workflow but places
credentials in this isolated workspace. Password fields use secure input and
values are never echoed afterward.

**Connectivity** contains manager identity, primary/alternative network manager
configuration, and supported LAN/asynchronous/WAN MTA topology information.
It is an operator/admin configuration workspace, separate from day-to-day
fault triage.

## State and interaction rules

### Command state vocabulary

Use a shared textual vocabulary everywhere:

| State | Operator meaning |
|---|---|
| Pending | Submitted or queued locally; awaiting a protocol outcome |
| Deferred | Remote participant returned `wait_ack`; a final result is expected |
| Received | A valid requested value/status was returned |
| Acknowledged | The command was accepted |
| Rejected | A NAK was returned; show the decoded reason |
| Timed out | No final response arrived within the configured policy |
| Delivery failed | Node Manager could not submit through its Router ingress |
| Cancelled locally | Operator stopped local waiting/retry; remote processing may continue |

Colour may supplement these labels but cannot be their sole representation.

### Confirmation and safety

- Reads and discovery requests are immediate actions.
- Remote-affecting actions require a review step.
- A Parameter change, reset, alert, test schedule change, bulk operation,
  alarm deletion, or explicit condition clear identifies its target and
  consequence before confirmation.
- Forms validate legal GD-92 values before sending, but participants remain
  the authority for protocol validation and access control.
- A NAK is always shown as the participant's response, including its reason
  code; the UI must not replace it with a generic “failed” state.

### Accessibility and responsive behavior

- Preserve semantic landmarks, headings, tables, labels, the skip link, and
  visible keyboard focus.
- Use a labelled status region for changes that do not move focus.
- Do not rely solely on colour, sound, hover, or animation for critical state.
- On narrow viewports, transform wide tables into labelled record cards or
  horizontally scrollable accessible tables; retain address, state, and action
  context.
- Keep the selected-address context and Activity control visible while the
  detail workspace scrolls.

## Roadmap mapping

| Roadmap phase | UI delivery |
|---|---|
| 1. General Parameter maintenance | Parameters workspace: Table selector, typed catalogue, paged values, edit review, and multi-target result view |
| 2. Common UA protocol behavior | Shared Activity state model, decoded NAKs, deferred work, abort, and standards-complete unsupported-message outcomes |
| 3. Durable alarms | Alarms workspace, Overview alarm summary, persistent lifecycle history, and notification indicator |
| 4. Commands and tests | Commands workspace, Run now test flow, Tests workspace, and durable schedules |
| 5. Statistics | Statistics workspace, threshold visualisation, and alarm links |
| 6. Network management scope | Network topology, remote address selection, capability display, and connectivity administration |
| 7. Resilience and UX | Durable activity, restart recovery presentation, two-second progress, abort, and health/availability surfaces |

## Implementation guardrails

- Deliver one workflow as an executable vertical slice. Do not create inactive
  UI controls for a future protocol handler.
- A participant owns its Parameters and validation. Node Manager uses GD-92
  messages and owns only management presentation, transactions, alarms,
  schedules, reports, and audit history.
- Keep static Agent Type/Parameter catalogues in a configurable presentation
  boundary. GD-92 has no generic remote catalogue-discovery message.
- Make the Activity tray and alarm history durable before relying on them for
  operator-visible state.
- Retain GD-92 vocabulary: Communications Address, Participant, User Agent,
  MTA, Parameter Table, ACK, NAK, and reason code.
- Keep protocol correlation, retries, priority, acknowledgement requirements,
  and reason-code handling out of view-specific JavaScript.
