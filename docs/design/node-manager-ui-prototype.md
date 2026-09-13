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
