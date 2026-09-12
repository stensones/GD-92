using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Stensones.GD92.StationEnd.Tests.Integration;

public sealed class RouterStartupPersistenceTests
{
	[Fact]
	public async Task AppHost_started_Router_initializes_its_existing_persistent_parameter_set()
	{
		var postgres = new PostgreSqlBuilder("postgres:17.5").Build();
		DistributedApplication? application = null;

		try
		{
			await postgres.StartAsync();
			var routerConnectionString = await CreateDatabaseAsync(postgres, "router");
			var lanMtaConnectionString = await CreateDatabaseAsync(postgres, "lan-mta");
			var printerUaConnectionString = await CreateDatabaseAsync(postgres, "printer-ua");
			var nodeManagerConnectionString = await CreateDatabaseAsync(postgres, "node-manager");
			var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.GD92_StationEnd_AppHost>(
					[
						"--Persistence:UseExternalPostgres=true",
						"--StationEnd:IncludeBusMTAAndIOUA=false",
						"--Parameters:router-level1-password=FIRE1",
						$"--ConnectionStrings:router-database={routerConnectionString}",
						$"--ConnectionStrings:lan-mta-database={lanMtaConnectionString}",
						$"--ConnectionStrings:printer-ua-database={printerUaConnectionString}",
						$"--ConnectionStrings:node-manager-database={nodeManagerConnectionString}"
					]);

			application = await appHost.BuildAsync();
			await application.StartAsync();
			await StationEndApplicationStartup.WaitForHealthyAsync(application, "Router");

			var parameterNumbers = await GetPermanentParameterNumbersAsync(application);

			parameterNumbers.Should().Equal(
				[1, 2, 3, 4, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]);
		}
		finally
		{
			if (application is not null)
			{
				await application.StopAsync();
				await application.DisposeAsync();
			}

			await postgres.DisposeAsync();
		}
	}

	private static async Task<string> CreateDatabaseAsync(PostgreSqlContainer postgres, string database)
	{
		await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand($"""CREATE DATABASE "{database}";""", connection);
		await command.ExecuteNonQueryAsync();

		return new NpgsqlConnectionStringBuilder(postgres.GetConnectionString())
		{
			Database = database
		}.ConnectionString;
	}

	private static async Task<byte[]> GetPermanentParameterNumbersAsync(
		DistributedApplication application)
	{
		var connectionString = await application.GetConnectionStringAsync("router-database");
		connectionString.Should().NotBeNull();

		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand(
			"""
			SELECT parameter_value."ParameterNumber"
			FROM node.parameter_value AS parameter_value
			INNER JOIN node.parameter_set AS parameter_set
				ON parameter_set."Id" = parameter_value."ParameterSetId"
			WHERE parameter_set."Kind" = 0
			ORDER BY parameter_value."ParameterNumber";
			""",
			connection);

		var parameterNumbers = new List<byte>();
		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			parameterNumbers.Add(reader.GetByte(0));
		}

		return parameterNumbers.ToArray();
	}
}
