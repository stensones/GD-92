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

builder.AddProject<Projects.Router>("Router")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	.WithReference(routerDatabase)
	.WaitFor(postgres)
	;

builder.AddProject<Projects.BusMTA>("bus-MTA");

builder.AddProject<Projects.IOUA>("IO-UA");

builder.AddProject<Projects.LANMTA>("LAN-MTA");

builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	.WithExternalHttpEndpoints();

builder.AddProject<Projects.PrinterUA>("Printer-UA");

builder.Build().Run();
