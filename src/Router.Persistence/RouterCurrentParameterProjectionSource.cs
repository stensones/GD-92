using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class RouterCurrentParameterProjectionSource
{
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.FromValue(1));

	private RouterCurrentParameterProjection? currentParameters;

	public void Publish(RouterCurrentParameterProjection projection)
	{
		ArgumentNullException.ThrowIfNull(projection);

		Interlocked.Exchange(ref this.currentParameters, projection);
	}

	public RouterCurrentParameterProjection GetCurrent()
	{
		return Volatile.Read(ref this.currentParameters) ??
			throw new InvalidOperationException("Router current Parameters have not been loaded.");
	}

	public bool TryLogOnAtLevelOne(PasswordParameter submittedPassword)
	{
		ArgumentNullException.ThrowIfNull(submittedPassword);

		if (submittedPassword.Level != LevelOne)
		{
			return false;
		}

		while (true)
		{
			var current = this.GetCurrent();
			if (!current.Level1PasswordVerifier.Verifies(submittedPassword.Password.Value))
			{
				return false;
			}

			var loggedOn = current.LogOnAtLevelOne(
				submittedPassword.CommunicationsAddress);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, loggedOn, current),
				current))
			{
				return true;
			}
		}
	}
}
