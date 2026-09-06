# Store participant-owned Parameter Values as opaque encoded bytes

Each parameter-owning Router, Message Transfer Agent, or User Agent persists its own Permanent and Non-Volatile GD-92 Parameter Tables as validated, canonical encoded Parameter Value bytes, located by Parameter Table, Parameter Number, and, where applicable, Parameter Entry Index. A participant's store contains values for only that participant's Communications Address. Its application owns the versioned Parameter catalogue, validates Parameter syntax and values, and builds its typed Current Parameter projection.

The Router's store is limited to Router-owned Parameters. Password values are stored there as salted verifiers, never raw passwords. The Router also owns the node-scoped, volatile Node Login and authorizes Parameter modification against it; a request to change a User Agent or Message Transfer Agent Parameter remains addressed to, validated by, and persisted by that participant.

Current Parameter values and Node Login remain volatile. On normal startup or reset, each participant initializes its Current Parameter Table from its own Non-Volatile Parameter Table. This avoids a large protocol-shaped EF Core mapping, retains exact GD-92 values, and allows reserved user-defined Parameters to remain opaque without creating cross-application database ownership.
