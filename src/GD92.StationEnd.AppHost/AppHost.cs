using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//	.WithLifetime(ContainerLifetime.Persistent);
//var db = sql.AddDatabase("database");

var useExternalPostgres = builder.Configuration.GetValue(
	"Persistence:UseExternalPostgres",
	false);
var rabbitMq = builder.AddRabbitMQ("RabbitMQ");
if (useExternalPostgres)
{
	rabbitMq.WithLifetime(ContainerLifetime.Session);
}
else
{
	rabbitMq.WithLifetime(ContainerLifetime.Persistent);
}

var routerLevel1Password = builder.AddParameterFromConfiguration(
	"router-level1-password",
	"Parameters:router-level1-password",
	secret: true);

if (useExternalPostgres)
{
	var routerDatabase = builder.AddConnectionString("router-database");
	var nodeManagerDatabase = builder.AddConnectionString("node-manager-database");

	var router = builder.AddProject<Projects.Router>("Router")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(routerDatabase)
		.WithEnvironment("Router__InitialLevel1Password", routerLevel1Password);

	builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(nodeManagerDatabase)
		.WaitFor(router);
}
else
{
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
	var nodeManagerDatabase = postgres.AddDatabase("node-manager-database", "node-manager");

	var router = builder.AddProject<Projects.Router>("Router")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(routerDatabase)
		.WaitFor(postgres)
		.WithEnvironment("Router__InitialLevel1Password", routerLevel1Password);

	builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(nodeManagerDatabase)
		.WaitFor(nodeManagerDatabase)
		.WaitFor(router);
}

builder.AddProject<Projects.BusMTA>("bus-MTA");

builder.AddProject<Projects.IOUA>("IO-UA");

builder.AddProject<Projects.LANMTA>("LAN-MTA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.AddProject<Projects.PrinterUA>("Printer-UA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.Build().Run();
