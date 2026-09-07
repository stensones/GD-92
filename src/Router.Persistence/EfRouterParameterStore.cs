using Microsoft.EntityFrameworkCore;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class EfRouterParameterStore : IParticipantParameterStore
{
	private const long InitializationLockKey = 4_914_215_198_316_848_129;
	private readonly RouterDbContext context;

	public EfRouterParameterStore(RouterDbContext context)
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
				parameterSet = await this.CreateRouterParameterSetsAsync(
					parameterTable,
					cancellationToken);
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
								"Router Parameter Store was initialized concurrently.");
						}

						var initializedParameterSet = await this.CreateRouterParameterSetsAsync(
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

	private async Task<ParameterSetRecord?> FindParameterSetAsync(
		ParameterTable parameterTable,
		CancellationToken cancellationToken)
	{
		return await PersistentRouterParameterSets.FindAsync(
			this.context,
			parameterTable,
			cancellationToken);
	}

	private async Task<ParameterSetRecord> CreateRouterParameterSetsAsync(
		ParameterTable requestedParameterTable,
		CancellationToken cancellationToken)
	{
		var node = new CommunicationsNodeRecord { Id = Guid.NewGuid() };
		var router = new ManagedEntityRecord
		{
			Id = Guid.NewGuid(),
			NodeId = node.Id,
			Kind = ManagedEntityKind.Router
		};
		var permanent = new ParameterSetRecord
		{
			Id = Guid.NewGuid(),
			ManagedEntityId = router.Id,
			Kind = PersistedParameterTableKind.Permanent
		};
		var nonVolatile = new ParameterSetRecord
		{
			Id = Guid.NewGuid(),
			ManagedEntityId = router.Id,
			Kind = PersistedParameterTableKind.NonVolatile
		};

		this.context.CommunicationsNodes.Add(node);
		this.context.ManagedEntities.Add(router);
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

}
