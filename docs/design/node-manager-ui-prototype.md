# Node Manager UI Prototype

The Node Manager is an operational console, not a general product dashboard.
Its UI should help an operator establish station context, select a management
task, and see the outcome without obscuring GD-92 terminology.

## Layout

The prototype in `src\NodeManager` is the visual reference:

- A persistent header identifies the product, station, node address, and
  operational state.
- A compact navigation rail groups future workflows without requiring a
  client-side router.
- The workspace opens with node context and uses panels for discrete operational
  tasks.
- Wide panels hold participant discovery and Parameter results, where tables
  are the natural representation.

## Interaction states

Use a visible status region for asynchronous GD-92 requests. Buttons must
remain enabled until an action begins, then be disabled only for the relevant
request. Tables should show explicit pending, received, rejected, and timed-out
states rather than omit results.

## Visual language

Use navy for application identity, teal for executable actions and normal
operational state, amber only for context or attention, and neutral surfaces
for data. Preserve contrast, keyboard focus indicators, semantic headings, and
the skip link as new workflows are added.

## Extending the prototype

New workflows should be added as a named navigation task and a self-contained
panel or detail workspace. Retain the existing vocabulary—Participant,
Inventory Scan, Router, User Agent, Parameter Table, and Communications
Address—rather than replacing it with generic UI terms.

## Implemented prototype surface

The Network workspace is available at `/network?address={communications-address}`.
It retains the selected Communications Address in the workspace context, exposes
the existing local Router Inventory Scan, and states explicitly that remote
catalogue discovery is unavailable until GD-92 provides a supporting workflow.
An active Inventory Scan announces progress in a labelled status region,
disables only its own action, displays discovered local Participants in a
result table, and retains received, rejected, timed-out, and delivery-failed
outcome counts.

The read-only Parameters workspace is available at
`/parameters?address={communications-address}&agentType={agent-type}`. It
retains the selected Communications Address and Agent Type, offers the
`Permanent`, `Non-Volatile`, and `Current` Parameter Tables, and explicitly
states that no values have been read until a Parameter request workflow is
implemented. The optional `parameterTable` query selects the matching table;
an invalid or absent selection defaults to `Current`. Selecting a table in the
browser preserves the selected Communications Address and Agent Type while
navigating to that query state.
For a selected local Router, the workspace can request the selected table's
Brigade or Agency Parameter through the existing GD-92 request workflow. Its labelled
status remains visible as Pending, Received, Rejected, Timed out, or Delivery
failed; it shows a returned value or rejection reason only when the Router
returns one.
The selected Router catalogue presently contains that Parameter as its first
typed row: Number, Name, Value, Read State, and Last Read. Its initial Read
State is `Not read`; a Last Read value appears only after a final request
outcome.
The workspace also provides a selected Routing Table range browser, initially
for entries `1` through `1`. It sends the existing `Param_req_multiple`
workflow, displays returned entry numbers and next nodes, and enables **Next
entries** only when the response signals `more_values`.
The persistent Network and Parameters navigation links retain the selected
Communications Address, and the active workspace is identified through the
navigation state as well as its page heading.
Each discovered local Participant offers **View Parameters**, carrying its
port-derived Communications Address and Agent Type into the Parameters
workspace. The local Router row does not present an unsupported Participant
Parameters action.
Recognized Participant Agent Types render their own Parameter catalogue rather
than Router content. The LAN MTA, Printer, and Network Management UA
catalogues use the selected Communications Address to validate and derive the
Participant port for their existing scalar Parameter request endpoint. A
missing, malformed, or unsupported Participant selection has an explicit
unavailable state; Router-only Routing Table controls remain exclusive to the
Router workspace.

The original `Home/Index.cshtml` test page remains unchanged.
