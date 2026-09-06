using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//	.WithLifetime(ContainerLifetime.Persistent);
//var db = sql.AddDatabase("database");

var rabbitMq = builder.AddRabbitMQ("RabbitMQ")
	.WithLifetime(ContainerLifetime.Persistent)
	;

var usePersistentPostgres = builder.Configuration.GetValue(
	"Persistence:UsePersistentPostgres",
	true);
var postgres = builder.AddPostgres("postgres");
if (usePersistentPostgres)
{
	postgres.WithDataVolume()
		.WithLifetime(ContainerLifetime.Persistent);
}
else
{
	postgres.WithLifetime(ContainerLifetime.Session);
}

var routerDatabase = postgres.AddDatabase("router-database", "router");
var routerLevel1Password = builder.AddParameter("router-level1-password", secret: true);

builder.AddProject<Projects.Router>("Router")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	.WithReference(routerDatabase)
	.WaitFor(postgres)
	.WithEnvironment("Router__InitialLevel1Password", routerLevel1Password)
	;

builder.AddProject<Projects.BusMTA>("bus-MTA");

builder.AddProject<Projects.IOUA>("IO-UA");

builder.AddProject<Projects.LANMTA>("LAN-MTA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.AddProject<Projects.PrinterUA>("Printer-UA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.Build().Run();
