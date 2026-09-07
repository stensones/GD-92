using System.Threading.Channels;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Testcontainers.PostgreSql;
using Wolverine;
using Wolverine.RabbitMQ;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class NodeManagerParameterPersistenceSteps
{
	private static readonly CommunicationsAddress NodeManagerAddress =
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));
	private static readonly CommunicationsAddress RouterAddress =
		CommunicationsAddress.FromValues(
			NodeManagerAddress.Brigade,
			NodeManagerAddress.Node,
			Port.FromValue(PortIdentifier.FromValue(0)));
	private static int nextRemoteUserAgentPort = 30;

	private DistributedApplication? application;
	private IHost? remoteUserAgent;
	private IConnection? remoteUserAgentConnection;
	private IChannel? remoteUserAgentChannel;
	private string? remoteUserAgentConsumerTag;
	private PostgreSqlContainer? postgres;
	private ParameterResponseReceiver? responses;
	private Envelope? response;
	private byte nextSequenceNumber;
	private readonly CommunicationsAddress remoteUserAgentAddress = CreateRemoteUserAgentAddress();

	[Given(@"an empty dedicated NodeManager Parameter Store")]
	public Task GivenAnEmptyDedicatedNodeManagerParameterStore()
	{
		return this.StartStationEndAsync();
	}

	[When(@"a remote User Agent requests NodeManager Current Parameter (.*)")]
	public async Task WhenARemoteUserAgentRequestsNodeManagerCurrentParameter(byte parameterNumber)
	{
		await this.StartRemoteUserAgentAsync();

		using var scope = this.remoteUserAgent!.Services.CreateScope();
		var routerIngress = scope.ServiceProvider.GetRequiredService<IRouterIngress>();
		var request = Envelope.FromValues(
			this.remoteUserAgentAddress,
			Destinations.FromAddresses(NodeManagerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(
					checked(++this.nextSequenceNumber))),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(parameterNumber)));

		await routerIngress.SubmitAsync(request, CancellationToken.None);
		this.response = await this.responses!.ReceiveAsync(
			request.AcknowledgementAndSequence.SequenceNumber);
	}

	[Then(@"NodeManager returns port number (.*)")]
	public void ThenNodeManagerReturnsPortNumber(byte portNumber)
	{
		this.GetReturnedParameterValue().Should().Equal([portNumber]);
	}

	[Then(@"NodeManager returns Network Management User Agent type (.*)")]
	public void ThenNodeManagerReturnsNetworkManagementUserAgentType(byte agentType)
	{
		this.GetReturnedParameterValue().Should().Equal([agentType]);
	}

	[When(@"NodeManager restarts")]
	public async Task WhenNodeManagerRestarts()
	{
		await this.StopRemoteUserAgentAsync();
		await this.application!.StopAsync();
		await this.application.DisposeAsync();
		this.application = null;
		await this.StartStationEndAsync();
	}

	[Then(@"NodeManager retains port number (.*) and Network Management User Agent type (.*) in its Permanent and Non-Volatile Parameter Tables")]
	public async Task ThenNodeManagerRetainsIdentityParameters(
		byte portNumber,
		byte agentType)
	{
		var connectionString = await this.application!.GetConnectionStringAsync(
			"node-manager-database");
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand(
			"""
			SELECT COUNT(*)
			FROM node.parameter_value AS parameter_value
			INNER JOIN node.parameter_set AS parameter_set
				ON parameter_set."Id" = parameter_value."ParameterSetId"
			WHERE parameter_set."Kind" IN (0, 1)
				AND (
					(parameter_value."ParameterNumber" = 1 AND parameter_value."EncodedValue" = @portNumber)
					OR
					(parameter_value."ParameterNumber" = 2 AND parameter_value."EncodedValue" = @agentType)
				);
			""",
			connection);
		command.Parameters.AddWithValue("portNumber", new byte[] { portNumber });
		command.Parameters.AddWithValue("agentType", new byte[] { agentType });

		(await command.ExecuteScalarAsync()).Should().Be(4L);
	}

	[AfterScenario]
	public async Task DisposeAsync()
	{
		await this.StopRemoteUserAgentAsync();

		if (this.application is not null)
		{
			await this.application.StopAsync();
			await this.application.DisposeAsync();
		}

		if (this.postgres is not null)
		{
			await this.postgres.DisposeAsync();
		}
	}

	private byte[] GetReturnedParameterValue()
	{
		this.response.Should().NotBeNull();
		this.response!.Source.Should().Be(NodeManagerAddress);
		this.response.Destinations.Addresses.Should().Equal(this.remoteUserAgentAddress);
		var parameter = this.response.Contents.Should().BeOfType<Parameter>().Subject;
		return parameter.ParameterValue.ToWireValue();
	}

	private async Task StartStationEndAsync()
	{
		if (this.application is not null)
		{
			return;
		}

		await this.StartPostgresAsync();
		var appHost = await DistributedApplicationTestingBuilder
			.CreateAsync<Projects.GD92_StationEnd_AppHost>(
				[
					"--Persistence:UseExternalPostgres=true",
					"--StationEnd:IncludeBusMTAAndIOUA=false",
					$"--ConnectionStrings:router-database={this.ConnectionStringFor("router")}",
					$"--ConnectionStrings:node-manager-database={this.ConnectionStringFor("node-manager")}"
				]);

		this.application = await appHost.BuildAsync();
		await this.application.StartAsync();
		await this.application.ResourceNotifications.WaitForResourceHealthyAsync("Router");
		await this.application.ResourceNotifications.WaitForResourceHealthyAsync("Node-Manager-UA");

		var rabbitMqConnectionString = await this.application.GetConnectionStringAsync("RabbitMQ")
			?? throw new InvalidOperationException("The test RabbitMQ connection string was not provided.");
		await WaitForQueueConsumerAsync(
			rabbitMqConnectionString,
			RouterIngressEndpoint.QueueNameFrom(RouterAddress),
			"Router ingress");
		await WaitForQueueConsumerAsync(
			rabbitMqConnectionString,
			LocalParticipantIngressEndpoint.QueueNameFrom(NodeManagerAddress),
			"NodeManager local participant ingress");
	}

	private async Task StartRemoteUserAgentAsync()
	{
		if (this.remoteUserAgent is not null)
		{
			return;
		}

		var rabbitMqConnectionString = await this.application!.GetConnectionStringAsync("RabbitMQ")
			?? throw new InvalidOperationException("The test RabbitMQ connection string was not provided.");
		var queueName = LocalParticipantIngressEndpoint.QueueNameFrom(this.remoteUserAgentAddress);
		this.responses = new ParameterResponseReceiver(rabbitMqConnectionString, queueName);
		await this.StartRemoteUserAgentListenerAsync(rabbitMqConnectionString, queueName);

		var builder = Host.CreateApplicationBuilder();
		builder.Configuration.AddInMemoryCollection(
			new Dictionary<string, string?>
			{
				["ConnectionStrings:RabbitMQ"] = rabbitMqConnectionString
			});
		builder.Services.AddWolverine(options =>
		{
			options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
		});
		builder.Services.AddScoped<IRouterIngress>(serviceProvider =>
			new RabbitMqRouterIngress(
				serviceProvider.GetRequiredService<IMessageBus>(),
				RouterAddress));

		this.remoteUserAgent = builder.Build();
		await this.remoteUserAgent.StartAsync();
	}

	private async Task StartRemoteUserAgentListenerAsync(
		string rabbitMqConnectionString,
		string queueName)
	{
		var factory = new ConnectionFactory
		{
			Uri = new Uri(rabbitMqConnectionString)
		};
		this.remoteUserAgentConnection = await factory.CreateConnectionAsync();
		this.remoteUserAgentChannel = await this.remoteUserAgentConnection.CreateChannelAsync();
		await this.remoteUserAgentChannel.QueueDeclareAsync(
			queueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null,
			passive: false,
			noWait: false,
			cancellationToken: CancellationToken.None);
		var consumer = new AsyncEventingBasicConsumer(this.remoteUserAgentChannel);
		consumer.ReceivedAsync += async (_, delivery) =>
		{
			var response = DecodeLocalParticipantIngressEnvelope(delivery.Body);
			await this.responses!.ReceiveAsync(response, delivery.CancellationToken);
			await this.remoteUserAgentChannel.BasicAckAsync(
				delivery.DeliveryTag,
				multiple: false,
				delivery.CancellationToken);
		};

		this.remoteUserAgentConsumerTag = await this.remoteUserAgentChannel.BasicConsumeAsync(
			queueName,
			autoAck: false,
			consumer,
			CancellationToken.None);
	}

	private async Task StartPostgresAsync()
	{
		if (this.postgres is not null)
		{
			return;
		}

		this.postgres = new PostgreSqlBuilder("postgres:17.5").Build();
		await this.postgres.StartAsync();
		await using var connection = new NpgsqlConnection(this.postgres.GetConnectionString());
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand(
			"""CREATE DATABASE "router"; CREATE DATABASE "node-manager";""",
			connection);
		await command.ExecuteNonQueryAsync();
	}

	private string ConnectionStringFor(string database)
	{
		var connectionString = new NpgsqlConnectionStringBuilder(this.postgres!.GetConnectionString())
		{
			Database = database
		};

		return connectionString.ConnectionString;
	}

	private async Task StopRemoteUserAgentAsync()
	{
		if (this.remoteUserAgentChannel is not null)
		{
			if (this.remoteUserAgentConsumerTag is not null)
			{
				await this.remoteUserAgentChannel.BasicCancelAsync(
					this.remoteUserAgentConsumerTag,
					noWait: false,
					CancellationToken.None);
			}

			await this.remoteUserAgentChannel.DisposeAsync();
			this.remoteUserAgentChannel = null;
			this.remoteUserAgentConsumerTag = null;
		}

		if (this.remoteUserAgentConnection is not null)
		{
			await this.remoteUserAgentConnection.DisposeAsync();
			this.remoteUserAgentConnection = null;
		}

		if (this.remoteUserAgent is null)
		{
			return;
		}

		await this.remoteUserAgent.StopAsync();
		this.remoteUserAgent.Dispose();
		this.remoteUserAgent = null;
		this.responses = null;
	}

	private static Envelope DecodeLocalParticipantIngressEnvelope(
		ReadOnlyMemory<byte> payload)
	{
		var message = (LocalParticipantIngressTransportMessage)
			LocalParticipantIngressTransportMessage.Read(payload.ToArray());
		var buffer = new EncodedMessageBuffer(message.EnvelopeWireValue);
		var envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException(
				"The encoded local participant ingress Envelope contains trailing bytes.");
		}

		return envelope;
	}

	private static CommunicationsAddress CreateRemoteUserAgentAddress()
	{
		var port = Interlocked.Increment(ref nextRemoteUserAgentPort);
		if (port > 63)
		{
			throw new InvalidOperationException("The test User Agent port range has been exhausted.");
		}

		return CommunicationsAddress.FromValues(
			NodeManagerAddress.Brigade,
			NodeManagerAddress.Node,
			Port.FromValue(PortIdentifier.FromValue(checked((byte)port))));
	}

	public sealed class ParameterResponseReceiver
	{
		private readonly Channel<Envelope> receivedEnvelopes = Channel.CreateUnbounded<Envelope>();
		private readonly List<ushort> receivedSequenceNumbers = [];
		private readonly string rabbitMqConnectionString;
		private readonly string localParticipantIngressQueue;

		public ParameterResponseReceiver(
			string rabbitMqConnectionString,
			string localParticipantIngressQueue)
		{
			this.rabbitMqConnectionString = rabbitMqConnectionString ??
				throw new ArgumentNullException(nameof(rabbitMqConnectionString));
			this.localParticipantIngressQueue = localParticipantIngressQueue ??
				throw new ArgumentNullException(nameof(localParticipantIngressQueue));
		}

		public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(envelope);
			cancellationToken.ThrowIfCancellationRequested();
			lock (this.receivedSequenceNumbers)
			{
				this.receivedSequenceNumbers.Add(envelope.AcknowledgementAndSequence.SequenceNumber.Value);
			}

			if (!this.receivedEnvelopes.Writer.TryWrite(envelope))
			{
				throw new InvalidOperationException("The test User Agent could not record a received Envelope.");
			}

			return Task.CompletedTask;
		}

		public async Task<Envelope> ReceiveAsync(SequenceNumber sequenceNumber)
		{
			ArgumentNullException.ThrowIfNull(sequenceNumber);

			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			try
			{
				while (true)
				{
					var envelope = await this.receivedEnvelopes.Reader.ReadAsync(timeout.Token);
					if (envelope.AcknowledgementAndSequence.SequenceNumber == sequenceNumber)
					{
						return envelope;
					}
				}
			}
			catch (OperationCanceledException exception) when (timeout.IsCancellationRequested)
			{
				var queue = await GetLocalParticipantIngressQueueStateAsync(
					this.rabbitMqConnectionString,
					this.localParticipantIngressQueue);
				throw new Xunit.Sdk.XunitException(
					$"NodeManager did not return a Parameter response through the local Router. " +
					$"The test receiver observed sequence numbers [{this.ReceivedSequenceNumbers()}]; " +
					$"the broker queue has {queue.MessageCount} message(s) and {queue.ConsumerCount} consumer(s).",
					exception);
			}
		}

		private string ReceivedSequenceNumbers()
		{
			lock (this.receivedSequenceNumbers)
			{
				return string.Join(", ", this.receivedSequenceNumbers);
			}
		}
	}

	private static async Task<LocalParticipantIngressQueueState>
		GetLocalParticipantIngressQueueStateAsync(
			string rabbitMqConnectionString,
			string localParticipantIngressQueue)
	{
		var factory = new ConnectionFactory
		{
			Uri = new Uri(rabbitMqConnectionString)
		};
		await using var connection = await factory.CreateConnectionAsync();
		await using var channel = await connection.CreateChannelAsync();
		var queue = await channel.QueueDeclarePassiveAsync(
			localParticipantIngressQueue);

		return new LocalParticipantIngressQueueState(queue.MessageCount, queue.ConsumerCount);
	}

	private static async Task WaitForQueueConsumerAsync(
		string rabbitMqConnectionString,
		string queueName,
		string ingressName)
	{
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

		while (!timeout.IsCancellationRequested)
		{
			try
			{
				var queue = await GetLocalParticipantIngressQueueStateAsync(
					rabbitMqConnectionString,
					queueName);
				if (queue.ConsumerCount > 0)
				{
					return;
				}
			}
			catch (OperationInterruptedException)
			{
				// The listener has not declared its queue yet.
			}

			try
			{
				await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
			}
			catch (OperationCanceledException) when (timeout.IsCancellationRequested)
			{
				break;
			}
		}

		throw new Xunit.Sdk.XunitException(
			$"{ingressName} did not start a RabbitMQ consumer within 30 seconds.");
	}

	private sealed record LocalParticipantIngressQueueState(
		uint MessageCount,
		uint ConsumerCount);
}
