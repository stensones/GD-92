using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//	.WithLifetime(ContainerLifetime.Persistent);
//var db = sql.AddDatabase("database");

var useExternalPostgres = builder.Configuration.GetValue(
	"Persistence:UseExternalPostgres",
	false);
var usePersistentPostgres = builder.Configuration.GetValue(
	"Persistence:UsePersistentPostgres",
	true);
var rabbitMq = builder.AddRabbitMQ("RabbitMQ");
if (useExternalPostgres || !usePersistentPostgres)
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
var routerLevel2Password = builder.AddParameterFromConfiguration(
	"router-level2-password",
	"Parameters:router-level2-password",
	secret: true);
var routerLevel3Password = builder.AddParameterFromConfiguration(
	"router-level3-password",
	"Parameters:router-level3-password",
	secret: true);
var routerLevel4Password = builder.AddParameterFromConfiguration(
	"router-level4-password",
	"Parameters:router-level4-password",
	secret: true);
var includeBusMtaAndIoUa = builder.Configuration.GetValue(
	"StationEnd:IncludeBusMTAAndIOUA",
	true);
var inventoryScanMaximumConcurrentProbes = builder.Configuration[
	"InventoryScan:MaximumConcurrentProbes"] ?? "8";
var enableRouterTestStateReset = builder.Configuration.GetValue(
	"Testing:EnableRouterStateReset",
	false);

if (useExternalPostgres)
{
	var routerDatabase = builder.AddConnectionString("router-database");
	var lanMtaDatabase = builder.AddConnectionString("lan-mta-database");
	var printerUaDatabase = builder.AddConnectionString("printer-ua-database");
	var nodeManagerDatabase = builder.AddConnectionString("node-manager-database");

	var router = builder.AddProject<Projects.Router>("Router")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(routerDatabase)
		.WithEnvironment("Router__InitialLevel1Password", routerLevel1Password)
		.WithEnvironment("Router__InitialLevel2Password", routerLevel2Password)
		.WithEnvironment("Router__InitialLevel3Password", routerLevel3Password)
		.WithEnvironment("Router__InitialLevel4Password", routerLevel4Password)
		.WithEnvironment(
			"Testing__EnableRouterStateReset",
			enableRouterTestStateReset.ToString())
		.WithHttpEndpoint(name: "http", env: "ASPNETCORE_HTTP_PORTS")
		.WithHttpHealthCheck("/health");

	builder.AddProject<Projects.LANMTA>("LAN-MTA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(lanMtaDatabase);

	builder.AddProject<Projects.PrinterUA>("Printer-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(printerUaDatabase)
		.WaitFor(printerUaDatabase);

	builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(nodeManagerDatabase)
		.WithHttpEndpoint(name: "http", env: "ASPNETCORE_HTTP_PORTS")
		.WithHttpsEndpoint(name: "https", env: "ASPNETCORE_HTTPS_PORTS")
		.WithHttpHealthCheck("/health")
		.WithEnvironment(
			"RouterParameterRequest__LocalRouter__Port",
			builder.Configuration["RouterParameterRequest:LocalRouter:Port"] ?? "0")
		.WithEnvironment(
			"GD92__no_ack_timeout",
			builder.Configuration["GD92:no_ack_timeout"] ?? "5")
		.WithEnvironment(
			"GD92__retries",
			builder.Configuration["GD92:retries"] ?? "3")
		.WithEnvironment(
			"InventoryScan__MaximumConcurrentProbes",
			inventoryScanMaximumConcurrentProbes)
		.WaitFor(router);
}
else
{
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
	var lanMtaDatabase = postgres.AddDatabase("lan-mta-database", "lan-mta");
	var printerUaDatabase = postgres.AddDatabase("printer-ua-database", "printer-ua");
	var nodeManagerDatabase = postgres.AddDatabase("node-manager-database", "node-manager");

	var router = builder.AddProject<Projects.Router>("Router")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(routerDatabase)
		.WaitFor(postgres)
		.WithEnvironment("Router__InitialLevel1Password", routerLevel1Password)
		.WithEnvironment("Router__InitialLevel2Password", routerLevel2Password)
		.WithEnvironment("Router__InitialLevel3Password", routerLevel3Password)
		.WithEnvironment("Router__InitialLevel4Password", routerLevel4Password)
		.WithEnvironment(
			"Testing__EnableRouterStateReset",
			enableRouterTestStateReset.ToString())
		.WithHttpEndpoint(name: "http", env: "ASPNETCORE_HTTP_PORTS")
		.WithHttpHealthCheck("/health");

	builder.AddProject<Projects.LANMTA>("LAN-MTA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(lanMtaDatabase)
		.WaitFor(lanMtaDatabase);

	builder.AddProject<Projects.PrinterUA>("Printer-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(printerUaDatabase)
		.WaitFor(printerUaDatabase);

	builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
		.WithReference(rabbitMq)
		.WaitFor(rabbitMq)
		.WithReference(nodeManagerDatabase)
		.WithHttpEndpoint(name: "http", env: "ASPNETCORE_HTTP_PORTS")
		.WithHttpsEndpoint(name: "https", env: "ASPNETCORE_HTTPS_PORTS")
		.WithHttpHealthCheck("/health")
		.WithEnvironment(
			"RouterParameterRequest__LocalRouter__Port",
			builder.Configuration["RouterParameterRequest:LocalRouter:Port"] ?? "0")
		.WithEnvironment(
			"GD92__no_ack_timeout",
			builder.Configuration["GD92:no_ack_timeout"] ?? "5")
		.WithEnvironment(
			"GD92__retries",
			builder.Configuration["GD92:retries"] ?? "3")
		.WithEnvironment(
			"InventoryScan__MaximumConcurrentProbes",
			inventoryScanMaximumConcurrentProbes)
		.WaitFor(nodeManagerDatabase)
		.WaitFor(router);
}

if (includeBusMtaAndIoUa)
{
	builder.AddProject<Projects.BusMTA>("bus-MTA");
	builder.AddProject<Projects.IOUA>("IO-UA");
}

builder.Build().Run();
