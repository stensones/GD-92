var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//	.WithLifetime(ContainerLifetime.Persistent);
//var db = sql.AddDatabase("database");

var rabbitMq = builder.AddRabbitMQ("RabbitMQ")
	.WithLifetime(ContainerLifetime.Persistent)
	;

builder.AddProject<Projects.Router>("Router")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	;

builder.AddProject<Projects.BusMTA>("bus-MTA");

builder.AddProject<Projects.IOUA>("IO-UA");

builder.AddProject<Projects.LANMTA>("LAN-MTA");

builder.AddProject<Projects.NodeManager>("Node-Manager-UA")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.AddProject<Projects.PrinterUA>("Printer-UA");

builder.Build().Run();
