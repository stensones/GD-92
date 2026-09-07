using Microsoft.EntityFrameworkCore;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class EfNodeManagerParameterStore : IParticipantParameterStore
{
	private const long InitializationLockKey = 3_824_961_050_823_119_454;
	private readonly NodeManagerDbContext context;

	public EfNodeManagerParameterStore(NodeManagerDbContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async ValueTask<ParameterValue?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken = default)
	{
		var parameterSet = await this.FindParameterSetAsync(parameterTable, cancellationToken);
		if (parameterSet is null)
		{
			return null;
		}

		var value = await this.context.ParameterValues.FindAsync(
			[parameterSet.Id, parameterNumber.Value],
			cancellationToken);
		return value is null ? null : ParameterValue.FromWireValue(value.EncodedValue);
	}

	public async ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		var parameterSet = await this.FindParameterSetAsync(parameterTable, cancellationToken);
		if (parameterSet is null)
		{
			if (this.context.Database.CurrentTransaction is not null)
			{
				parameterSet = await this.CreateParameterSetsAsync(parameterTable, cancellationToken);
			}
			else
			{
				await this.ExecuteInitializationAsync(
					async initializeCancellationToken =>
					{
						if (await this.FindParameterSetAsync(
							parameterTable,
							initializeCancellationToken) is not null)
						{
							throw new DbUpdateConcurrencyException(
								"NodeManager Parameter Store was initialized concurrently.");
						}

						var initializedParameterSet = await this.CreateParameterSetsAsync(
							parameterTable,
							initializeCancellationToken);
						await this.StoreInParameterSetAsync(
							initializedParameterSet,
							parameterNumber,
							parameterValue,
							initializeCancellationToken);

						return true;
					},
					cancellationToken);
				return;
			}
		}

		await this.StoreInParameterSetAsync(
			parameterSet,
			parameterNumber,
			parameterValue,
			cancellationToken);
	}

	public async ValueTask<T> ExecuteInitializationAsync<T>(
		Func<CancellationToken, ValueTask<T>> initialize,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(initialize);

		return await this.context.Database.CreateExecutionStrategy().ExecuteAsync(
			async () =>
			{
				await using var transaction = await this.context.Database.BeginTransactionAsync(
					cancellationToken);
				await this.context.Database.ExecuteSqlRawAsync(
					"SELECT pg_advisory_xact_lock({0});",
					[InitializationLockKey],
					cancellationToken);

				var result = await initialize(cancellationToken);
				await transaction.CommitAsync(cancellationToken);

				return result;
			});
	}

	private async Task StoreInParameterSetAsync(
		ParameterSetRecord parameterSet,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue,
		CancellationToken cancellationToken)
	{
		var value = await this.context.ParameterValues.FindAsync(
			[parameterSet.Id, parameterNumber.Value],
			cancellationToken);
		if (value is null)
		{
			this.context.ParameterValues.Add(new PersistedParameterValueRecord
			{
				ParameterSetId = parameterSet.Id,
				ParameterNumber = parameterNumber.Value,
				EncodedValue = parameterValue.ToWireValue()
			});
		}
		else
		{
			value.EncodedValue = parameterValue.ToWireValue();
			parameterSet.Revision++;
		}

		await this.context.SaveChangesAsync(cancellationToken);
	}

	private Task<ParameterSetRecord?> FindParameterSetAsync(
		ParameterTable parameterTable,
		CancellationToken cancellationToken)
	{
		var kind = ToPersistedKind(parameterTable);
		return this.context.ParameterSets.SingleOrDefaultAsync(
			parameterSet => parameterSet.Kind == kind,
			cancellationToken);
	}

	private async Task<ParameterSetRecord> CreateParameterSetsAsync(
		ParameterTable requestedParameterTable,
		CancellationToken cancellationToken)
	{
		var permanent = new ParameterSetRecord
		{
			Id = Guid.NewGuid(),
			Kind = PersistedParameterTableKind.Permanent
		};
		var nonVolatile = new ParameterSetRecord
		{
			Id = Guid.NewGuid(),
			Kind = PersistedParameterTableKind.NonVolatile
		};

		this.context.ParameterSets.AddRange(permanent, nonVolatile);
		await this.context.SaveChangesAsync(cancellationToken);
		return requestedParameterTable == ParameterTable.Permanent
			? permanent
			: requestedParameterTable == ParameterTable.NonVolatile
				? nonVolatile
				: throw new ArgumentOutOfRangeException(
					nameof(requestedParameterTable),
					"Only permanent and non-volatile Parameter Tables are persisted.");
	}

	private static PersistedParameterTableKind ToPersistedKind(ParameterTable parameterTable)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);

		return parameterTable == ParameterTable.Permanent
			? PersistedParameterTableKind.Permanent
			: parameterTable == ParameterTable.NonVolatile
				? PersistedParameterTableKind.NonVolatile
				: throw new ArgumentOutOfRangeException(
					nameof(parameterTable),
					"Only permanent and non-volatile Parameter Tables are persisted.");
	}
}
