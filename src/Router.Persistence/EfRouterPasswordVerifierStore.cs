using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class EfRouterPasswordVerifierStore : IRouterPasswordVerifierStore
{
	private static readonly ParameterNumber Level1PasswordParameterNumber =
		ParameterNumber.FromValue(5);

	private readonly RouterDbContext context;

	public EfRouterPasswordVerifierStore(RouterDbContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken = default)
	{
		EnsureLevel1PasswordParameterNumber(parameterNumber);

		var parameterSet = await PersistentRouterParameterSets.FindAsync(
			this.context,
			parameterTable,
			cancellationToken);
		if (parameterSet is null)
		{
			return null;
		}

		var record = await this.context.PasswordVerifiers.FindAsync(
			[parameterSet.Id, parameterNumber.Value],
			cancellationToken);

		return record is null ? null : PasswordVerifier.FromStoredData(
			PasswordVerifierData.FromStoredValues(
				PasswordVerifierVersion.FromValue(record.Version),
				PasswordVerifierWorkFactor.FromIterations(record.WorkFactor),
				record.Salt,
				record.Hash));
	}

	public async ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(passwordVerifier);
		EnsureLevel1PasswordParameterNumber(parameterNumber);

		var parameterSet = await PersistentRouterParameterSets.FindAsync(
			this.context,
			parameterTable,
			cancellationToken)
			?? throw new InvalidOperationException(
				"A password verifier requires an existing persistent Router Parameter Set.");
		var data = passwordVerifier.ToStoredData();
		var record = await this.context.PasswordVerifiers.FindAsync(
			[parameterSet.Id, parameterNumber.Value],
			cancellationToken);

		if (record is null)
		{
			this.context.PasswordVerifiers.Add(new PasswordVerifierRecord
			{
				ParameterSetId = parameterSet.Id,
				ParameterNumber = parameterNumber.Value,
				Version = data.Version.Value,
				WorkFactor = data.WorkFactor.Iterations,
				Salt = data.Salt,
				Hash = data.Hash
			});
		}
		else
		{
			record.Version = data.Version.Value;
			record.WorkFactor = data.WorkFactor.Iterations;
			record.Salt = data.Salt;
			record.Hash = data.Hash;
		}

		await this.context.SaveChangesAsync(cancellationToken);
	}

	private static void EnsureLevel1PasswordParameterNumber(ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		if (parameterNumber != Level1PasswordParameterNumber)
		{
			throw new ArgumentOutOfRangeException(
				nameof(parameterNumber),
				"Only Router Parameter 5 stores a password verifier.");
		}
	}
}
