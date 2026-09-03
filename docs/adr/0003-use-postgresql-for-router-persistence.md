# Use PostgreSQL for Router persistence

StationEnd uses a persistent PostgreSQL Docker resource named `postgres`, with a Router-owned database named `router`, because PostgreSQL has official Aspire and Npgsql EF Core integration while avoiding SQL Server container licensing and resource overhead. Only Router receives the database connection; NodeManager accesses Router-owned protocol state through GD-92 requests, and EF Core is deferred until a Router persistence model exists.
