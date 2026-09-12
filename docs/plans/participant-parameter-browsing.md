# Participant Parameter Browsing

## Goal

Let a Node Manager user browse GD-92 Parameters for discovered local participants
without reading another participant's database directly.

## Protocol model

GD-92 Message 61 (`Parameter_request`) reads one identified Parameter from a
selected Parameter Table. It does not enumerate a participant's catalogue. Node
Manager therefore owns a static browse catalogue selected from the Agent Type
obtained during Inventory Scan.

The initial browser table is `Current`, because it is the effective operational
Parameter Table. The browser will later allow `Permanent` and `Non-Volatile`
selection because Message 61 permits any Parameter Table.

Each selected scalar Parameter is requested in an individual Message 61. The
participant returns Message 62 (`Parameter`) using the original sequence number
and priority, with `ack_req` cleared. Invalid Parameter Numbers and Parameter
Tables return a parameter `NAK`; they must not be silently ignored.

Table-shaped Parameters use Message 63 (`Param_req_multiple`) and page through
the selected entries. `more_values` indicates that the user may request the next
page.

## Browse catalogues

| Participant | Current Parameters |
|---|---|
| Router | 1-21; 5-8 always display `PASSWORD` |
| LAN MTA | 1-10, 21 |
| Printer UA | 1-3, 21-24 |
| Network Management UA | 1-3 |

The catalogue is Node Manager presentation and request metadata. It is not
discovered remotely, and it does not create shared database ownership.

## Delivery slices

1. Introduce a local-participant Parameter Request path in Node Manager that
   accepts the selected local port and Parameter Number, while retaining
   Router-specific logon and logoff paths.
2. Make every modeled participant answer its complete Current Parameter
   catalogue. Router Password Parameters remain redacted as `PASSWORD`.
3. Add the Agent Type-selected browser view and wire every discovered
   participant's `View parameters` button to the local-participant request path.
4. Add typed value formatting for scalar Parameters and structured, paged table
   browsing through Message 63.
5. Add `Permanent` and `Non-Volatile` table selection, then support Parameter
   modification as a separately authorised capability.

## Acceptance criteria

After Inventory Scan discovers Router, LAN MTA, Printer UA, and Network
Management UA, selecting `View parameters` displays each participant's complete
Current catalogue. Invalid requests display the returned parameter `NAK`.
Password values are never exposed. Table browsing follows Message 63 entry
selection and `more_values`.
