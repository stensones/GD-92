using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class EfRouterPasswordVerifierStore : IRouterLevel1PasswordVerifierStore
{
	private readonly RouterDbContext context;

	public EfRouterPasswordVerifierStore(RouterDbContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		CancellationToken cancellationToken = default)
	{
		var parameterSet = await PersistentRouterParameterSets.FindAsync(
			this.context,
			parameterTable,
			cancellationToken);
		if (parameterSet is null)
		{
			return null;
		}

		var record = await this.context.PasswordVerifiers.FindAsync(
			[parameterSet.Id, RouterParameterCatalogue.Level1PasswordNumber.Value],
			cancellationToken);

		return record is null ? null : PasswordVerifier.FromStoredData(
			PasswordVerifierData.Create(
				PasswordVerifierVersion.FromDatabaseValue(record.Version),
				PasswordVerifierWorkFactor.FromDatabaseValue(record.WorkFactor),
				PasswordVerifierSalt.FromDatabaseValue(record.Salt),
				PasswordVerifierHash.FromDatabaseValue(record.Hash)));
	}

	public async ValueTask StoreAsync(
		ParameterTable parameterTable,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(passwordVerifier);

		var parameterSet = await PersistentRouterParameterSets.FindAsync(
			this.context,
			parameterTable,
			cancellationToken)
			?? throw new InvalidOperationException(
				"A password verifier requires an existing persistent Router Parameter Set.");
		var data = passwordVerifier.ToStoredData();
		var record = await this.context.PasswordVerifiers.FindAsync(
			[parameterSet.Id, RouterParameterCatalogue.Level1PasswordNumber.Value],
			cancellationToken);

		if (record is null)
		{
			this.context.PasswordVerifiers.Add(new PasswordVerifierRecord
			{
				ParameterSetId = parameterSet.Id,
				ParameterNumber = RouterParameterCatalogue.Level1PasswordNumber.Value,
				Version = data.Version.ToDatabaseValue(),
				WorkFactor = data.WorkFactor.ToDatabaseValue(),
				Salt = data.Salt.ToDatabaseValue(),
				Hash = data.Hash.ToDatabaseValue()
			});
		}
		else
		{
			record.Version = data.Version.ToDatabaseValue();
			record.WorkFactor = data.WorkFactor.ToDatabaseValue();
			record.Salt = data.Salt.ToDatabaseValue();
			record.Hash = data.Hash.ToDatabaseValue();
		}
		parameterSet.Revision++;

		await this.context.SaveChangesAsync(cancellationToken);
	}
}
