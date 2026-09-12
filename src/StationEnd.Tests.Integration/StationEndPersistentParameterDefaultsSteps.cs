using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Npgsql;
using Reqnroll;
using Testcontainers.PostgreSql;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class StationEndPersistentParameterDefaultsSteps
{
	private static readonly TimeSpan ParameterSchemaReadinessTimeout = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan ParameterSchemaPollInterval = TimeSpan.FromMilliseconds(250);

	private static readonly IReadOnlyDictionary<string, byte[]> ExpectedParameterNumbers =
		new Dictionary<string, byte[]>
		{
			["router-database"] = [1, 2, 3, 4, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21],
			["lan-mta-database"] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 21],
			["printer-ua-database"] = [1, 2, 3, 21, 22, 23, 24],
			["node-manager-database"] = [1, 2, 3]
		};

	private DistributedApplication? application;
	private PostgreSqlContainer? postgres;

	[Given(@"empty dedicated Parameter Stores for the modeled Station End participants")]
	public async Task GivenEmptyDedicatedParameterStoresForTheModeledStationEndParticipants()
	{
		this.postgres = new PostgreSqlBuilder("postgres:17.5").Build();
		await this.postgres.StartAsync();

		await using var connection = new NpgsqlConnection(this.postgres.GetConnectionString());
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand(
			"""
			CREATE DATABASE "router";
			CREATE DATABASE "lan-mta";
			CREATE DATABASE "printer-ua";
			CREATE DATABASE "node-manager";
			""",
			connection);
		await command.ExecuteNonQueryAsync();
	}

	[When(@"the Station End solution starts with its default configuration")]
	public async Task WhenTheStationEndSolutionStartsWithItsDefaultConfiguration()
	{
		var appHost = await DistributedApplicationTestingBuilder
			.CreateAsync<Projects.GD92_StationEnd_AppHost>(
				[
					"--Persistence:UseExternalPostgres=true",
					"--StationEnd:IncludeBusMTAAndIOUA=false",
					"--Parameters:router-level1-password=FIRE1",
					$"--ConnectionStrings:router-database={this.ConnectionStringFor("router")}",
					$"--ConnectionStrings:lan-mta-database={this.ConnectionStringFor("lan-mta")}",
					$"--ConnectionStrings:printer-ua-database={this.ConnectionStringFor("printer-ua")}",
					$"--ConnectionStrings:node-manager-database={this.ConnectionStringFor("node-manager")}"
				]);

		this.application = await appHost.BuildAsync();
		await this.application.StartAsync();
		await StationEndApplicationStartup.WaitForHealthyAsync(
			this.application,
			"Router",
			"LAN-MTA",
			"Printer-UA",
			"Node-Manager-UA");
	}

	[Then(@"Router, LAN MTA, Printer UA, and Node Manager retain their complete default Permanent Parameter Tables")]
	public async Task ThenParticipantsRetainTheirCompleteDefaultPermanentParameterTables()
	{
		foreach (var (databaseResource, expectedParameterNumbers) in ExpectedParameterNumbers)
		{
			var actualParameterNumbers = await this.GetParameterNumbersAsync(
				databaseResource,
				parameterTableKind: 0);

			actualParameterNumbers.Should().Equal(expectedParameterNumbers);
		}
	}

	[Then(@"their Non-Volatile Parameter Tables contain the same Parameter Numbers")]
	public async Task ThenTheirNonVolatileParameterTablesContainTheSameParameterNumbers()
	{
		foreach (var (databaseResource, expectedParameterNumbers) in ExpectedParameterNumbers)
		{
			var actualParameterNumbers = await this.GetParameterNumbersAsync(
				databaseResource,
				parameterTableKind: 1);

			actualParameterNumbers.Should().Equal(expectedParameterNumbers);
		}
	}

	[AfterScenario]
	public async Task DisposeAsync()
	{
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

	private async Task<byte[]> GetParameterNumbersAsync(string databaseResource, byte parameterTableKind)
	{
		var connectionString = await this.application!.GetConnectionStringAsync(databaseResource);
		connectionString.Should().NotBeNull();

		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();
		await this.WaitForParameterSchemaAsync(connection, databaseResource);
		await using var command = new NpgsqlCommand(
			"""
			SELECT parameter_value."ParameterNumber"
			FROM node.parameter_value AS parameter_value
			INNER JOIN node.parameter_set AS parameter_set
				ON parameter_set."Id" = parameter_value."ParameterSetId"
			WHERE parameter_set."Kind" = @parameterTableKind
			ORDER BY parameter_value."ParameterNumber";
			""",
			connection);
		command.Parameters.AddWithValue("parameterTableKind", parameterTableKind);

		var parameterNumbers = new List<byte>();
		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			parameterNumbers.Add(reader.GetByte(0));
		}

		return parameterNumbers.ToArray();
	}

	private async Task WaitForParameterSchemaAsync(NpgsqlConnection connection, string databaseResource)
	{
		using var timeout = new CancellationTokenSource(ParameterSchemaReadinessTimeout);
		try
		{
			while (true)
			{
				try
				{
					await using var command = new NpgsqlCommand(
						"""
						SELECT 1
						FROM node.parameter_set
						CROSS JOIN node.parameter_value
						LIMIT 1;
						""",
						connection);
					await command.ExecuteScalarAsync(timeout.Token);
					return;
				}
				catch (PostgresException exception) when (
					exception.SqlState == PostgresErrorCodes.UndefinedTable)
				{
					await Task.Delay(ParameterSchemaPollInterval, timeout.Token);
				}
			}
		}
		catch (OperationCanceledException exception) when (timeout.IsCancellationRequested)
		{
			throw new TimeoutException(
				$"Timed out after {ParameterSchemaReadinessTimeout} waiting for node.parameter_set " +
				$"and node.parameter_value to exist in '{databaseResource}'.",
				exception);
		}
	}

	private string ConnectionStringFor(string database)
	{
		var connectionString = new NpgsqlConnectionStringBuilder(this.postgres!.GetConnectionString())
		{
			Database = database
		};

		return connectionString.ConnectionString;
	}
}
