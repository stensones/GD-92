using Microsoft.EntityFrameworkCore;
using Stensones.GD92.Fields;

namespace Router.Persistence;

internal static class PersistentRouterParameterSets
{
	public static Task<ParameterSetRecord?> FindAsync(
		RouterDbContext context,
		ParameterTable parameterTable,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);

		var kind = ToPersistedKind(parameterTable);
		return (
			from managedEntity in context.ManagedEntities
			join parameterSet in context.ParameterSets on managedEntity.Id equals parameterSet.ManagedEntityId
			where managedEntity.Kind == ManagedEntityKind.Router && parameterSet.Kind == kind
			select parameterSet).SingleOrDefaultAsync(cancellationToken);
	}

	public static PersistedParameterTableKind ToPersistedKind(ParameterTable parameterTable)
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
