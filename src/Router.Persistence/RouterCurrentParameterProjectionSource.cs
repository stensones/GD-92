using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class RouterCurrentParameterProjectionSource
{
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.Level1);

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

	public bool TryLogOffAtLevelZero(
		CommunicationsAddress localAddress)
	{
		ArgumentNullException.ThrowIfNull(localAddress);

		while (true)
		{
			var current = this.GetCurrent();
			if (ReferenceEquals(
				Interlocked.CompareExchange(
					ref this.currentParameters,
					current.LogOffAtLevelZero(localAddress),
					current),
				current))
			{
				return true;
			}
		}
	}

	public bool HasActiveNodeLoginAtLevelOne()
	{
		return this.GetCurrent().CurrentPassword.Level == LevelOne;
	}

	public bool TryChangeLevel1Password(PasswordVerifier passwordVerifier)
	{
		ArgumentNullException.ThrowIfNull(passwordVerifier);

		while (true)
		{
			var current = this.GetCurrent();
			if (current.CurrentPassword.Level != LevelOne)
			{
				return false;
			}

			var changed = current.WithLevel1PasswordVerifier(passwordVerifier);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, changed, current),
				current))
			{
				return true;
			}
		}
	}

	public bool TryChangeRetries(Retries retries)
	{
		ArgumentNullException.ThrowIfNull(retries);

		while (true)
		{
			var current = this.GetCurrent();
			if (current.CurrentPassword.Level != LevelOne)
			{
				return false;
			}

			var changed = current.WithRetries(retries);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, changed, current),
				current))
			{
				return true;
			}
		}
	}

	public bool TryChangeNoAcknowledgementTimeout(
		NoAcknowledgementTimeout noAcknowledgementTimeout)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);

		while (true)
		{
			var current = this.GetCurrent();
			if (current.CurrentPassword.Level != LevelOne)
			{
				return false;
			}

			var changed = current.WithNoAcknowledgementTimeout(noAcknowledgementTimeout);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, changed, current),
				current))
			{
				return true;
			}
		}
	}
}
