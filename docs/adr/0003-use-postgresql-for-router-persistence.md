# Use PostgreSQL for Router persistence

StationEnd uses a persistent PostgreSQL Docker resource named `postgres`, because PostgreSQL has official Aspire and Npgsql EF Core integration while avoiding SQL Server container licensing and resource overhead. It provisions application-owned databases named `router`, `lan-mta`, `printer-ua`, and `node-manager` for Router, LAN MTA, Printer UA, and NodeManager respectively. Only Router receives the `router` database connection; NodeManager accesses Router-owned protocol state through GD-92 requests.

The PostgreSQL resource is deployment infrastructure, not a shared Parameter Table. Router, LAN MTA, Printer UA, and NodeManager each receive their own logical database, connection, migrations, and store. It does not read or write another participant's Parameter Tables. Co-hosting those stores on the same PostgreSQL server is permitted, but does not change their application ownership.

No participant database is provisioned before that participant implements a mutable or durable Parameter. Its first persistence slice must add its own Permanent and Non-Volatile Parameter Table lifecycle.
