using Microsoft.EntityFrameworkCore;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class EfNodeManagerParameterStore : IParticipantParameterStore
{
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

		var parameterSet = await this.FindParameterSetAsync(parameterTable, cancellationToken)
			?? await this.CreateParameterSetsAsync(cancellationToken);
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
		return permanent;
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
