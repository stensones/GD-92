# Participant Parameter Browsing

## Goal

Let a Node Manager user browse GD-92 Parameters for discovered local participants
without reading another participant's database directly.

## Protocol model

GD-92 Message 61 (`Parameter_request`) reads one identified Parameter from a
selected Parameter Table. It does not enumerate a participant's catalogue. Node
Manager therefore owns a static browse catalogue selected from the Agent Type
obtained during Inventory Scan.

The browser defaults to `Current`, because it is the effective operational
Parameter Table, and lets the operator select `Permanent` or `Non-Volatile`
because Message 61 permits any Parameter Table. LAN MTA currently supports
scalar reads from all three tables; other participants advertise the same
selection UI but must implement each table according to their owned Parameter
semantics.

Each selected scalar Parameter is requested in an individual Message 61. The
participant returns Message 62 (`Parameter`) using the original sequence number
and priority, with `ack_req` cleared. Invalid Parameter Numbers and Parameter
Tables require a parameter `NAK`; they must not be silently ignored.

Table-shaped Parameters use Message 63 (`Param_req_multiple`) and page through
the selected entries. Router Current Parameter 13 (`Routing Table`) currently
supports a requested entry range and returns decoded entry index and next-node
values through Node Manager's status API. The Router returns `more_values` only
when all requested entries cannot fit in the 1,023-byte GD-92 Parameter message
content limit. The browser then enables **Next entries**, requesting from the
index after the last returned entry through the original upper bound; it hides
and disables the control on the final page.
Requesting a Router Routing Table range that contains any missing entry returns
`NAK(parameter / inv_entry)`, rather than a partial Parameter response. Node
Manager exposes this as `Parameter / Invalid Entry` and displays it in the
Routing Table request status. Broader invalid-table, Parameter-number, field,
and participant validation remains pending.

Router rejects a Current-table scalar request outside its defined Parameter
Numbers 1-21 with `NAK(parameter / inv_param)`. Node Manager exposes this as
`Parameter / Invalid Parameter`. Invalid Parameter Tables, fields, and
equivalent behavior for other participants remain pending.

Router Current Parameter 2 (`node_number`) is decoded as GD-92 `word16` and
presented as the local Router Node Number. For example, Router address
`26.100.0` returns `100`.

Router Current Parameter 3 (`node_name`) is decoded using the GD-92 typed
`NodeName` field and presented from the Router's configured node name. For
example, `Router:NodeName = Station End` returns `Station End`.

Router Current Parameter 9 (`maximum_message_length`) is decoded using the
GD-92 typed `MaximumMessageLength` field and presented from the Router's
configured maximum message length. For example,
`Router:MaximumMessageLength = 1023` returns `1023`.

Router Current Parameter 10 (`network_manager_address_1`) is decoded using the
GD-92 typed `CommunicationsAddress` field and presented from the Router's first
configured Network Manager address. For example,
`Router:NetworkManagerAddress1 = 26.100.25` returns `26.100.25`.

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
5. Completed for LAN MTA scalar Parameters: add `Permanent` and
   `Non-Volatile` table selection and participant-owned retained reads. Support
   Parameter modification as a separately authorised capability.

## Acceptance criteria

After Inventory Scan discovers Router, LAN MTA, Printer UA, and Network
Management UA, selecting `View parameters` displays each participant's complete
Current catalogue. Invalid requests display the returned parameter `NAK`.
Password values are never exposed. Table browsing follows Message 63 entry
selection and `more_values`.
