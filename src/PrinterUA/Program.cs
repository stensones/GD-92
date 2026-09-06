using PrinterUA;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stensones.GD92.Fields;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var localAddress = CommunicationsAddress.FromValues(
	Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
	Node.FromValue(NodeIdentifier.FromValue(100)),
	Port.FromValue(PortIdentifier.FromValue(2)));
var localRouter = CommunicationsAddress.FromValues(
	localAddress.Brigade,
	localAddress.Node,
	Port.FromValue(PortIdentifier.FromValue(0)));
var protocolVersion = ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2));

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForLocalParticipantIngress(localAddress);
});
builder.Services.AddSingleton(new PrinterUaSettings(localAddress, localRouter, protocolVersion));
builder.Services.AddScoped<IRouterIngress, PrinterUaRouterIngress>();
builder.Services.AddScoped<ILocalParticipantIngressReceiver, PrinterUaParameterReceiver>();

await builder.Build().RunAsync();
