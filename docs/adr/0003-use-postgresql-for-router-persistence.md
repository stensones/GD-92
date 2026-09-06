# Use PostgreSQL for Router persistence

StationEnd uses a persistent PostgreSQL Docker resource named `postgres`, with a Router-owned database named `router`, because PostgreSQL has official Aspire and Npgsql EF Core integration while avoiding SQL Server container licensing and resource overhead. Only Router receives the `router` database connection; NodeManager accesses Router-owned protocol state through GD-92 requests.

The PostgreSQL resource is deployment infrastructure, not a shared Parameter Table. A Router, Message Transfer Agent, or User Agent that later needs durable GD-92 Parameters receives its own logical database or schema, connection, migrations, and store. It does not read or write another participant's Parameter Tables. Co-hosting those stores on the same PostgreSQL server is permitted, but does not change their application ownership.

No participant database is provisioned before that participant implements a mutable or durable Parameter. Its first persistence slice must add its own Permanent and Non-Volatile Parameter Table lifecycle.
