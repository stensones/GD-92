using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class RouterCurrentParameterProjectionSource
{
	private static readonly PasswordLevel LevelOne =
		PasswordLevel.FromValue(PasswordLevelNumber.Level1);
	private static readonly PasswordLevel LevelTwo =
		PasswordLevel.FromValue(PasswordLevelNumber.Level2);
	private static readonly PasswordLevel LevelThree =
		PasswordLevel.FromValue(PasswordLevelNumber.Level3);
	private static readonly PasswordLevel LevelFour =
		PasswordLevel.FromValue(PasswordLevelNumber.Level4);

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

		return this.TryLogOn(submittedPassword);
	}

	public bool TryLogOn(PasswordParameter submittedPassword)
	{
		ArgumentNullException.ThrowIfNull(submittedPassword);

		while (true)
		{
			var current = this.GetCurrent();
			var passwordVerifier = submittedPassword.Level switch
			{
				var level when level == LevelOne => current.Level1PasswordVerifier,
				var level when level == LevelTwo => current.Level2PasswordVerifier,
				var level when level == LevelThree => current.Level3PasswordVerifier,
				var level when level == LevelFour => current.Level4PasswordVerifier,
				_ => null
			};
			if (passwordVerifier is null ||
				!passwordVerifier.Verifies(submittedPassword.Password.Value))
			{
				return false;
			}

			var loggedOn = current.LogOn(
				submittedPassword.Level,
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

	public bool HasActiveNodeLoginAtOrAbove(
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		ArgumentNullException.ThrowIfNull(requiredPasswordLevel);
		ArgumentNullException.ThrowIfNull(sourceAddress);

		return IsAuthorized(
			this.GetCurrent(),
			requiredPasswordLevel,
			sourceAddress);
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

	public bool TryChangeRetries(
		Retries retries,
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		ArgumentNullException.ThrowIfNull(retries);
		ArgumentNullException.ThrowIfNull(requiredPasswordLevel);
		ArgumentNullException.ThrowIfNull(sourceAddress);

		while (true)
		{
			var current = this.GetCurrent();
			if (!IsAuthorized(current, requiredPasswordLevel, sourceAddress))
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

	public bool TryChangeMaximumMessageLength(
		MaximumMessageLength maximumMessageLength,
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		ArgumentNullException.ThrowIfNull(maximumMessageLength);
		ArgumentNullException.ThrowIfNull(requiredPasswordLevel);
		ArgumentNullException.ThrowIfNull(sourceAddress);

		while (true)
		{
			var current = this.GetCurrent();
			if (!IsAuthorized(current, requiredPasswordLevel, sourceAddress))
			{
				return false;
			}

			var changed = current.WithMaximumMessageLength(maximumMessageLength);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, changed, current),
				current))
			{
				return true;
			}
		}
	}

	private static bool IsAuthorized(
		RouterCurrentParameterProjection current,
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		return current.CurrentPassword.Level.Value >= requiredPasswordLevel.Value &&
			current.CurrentPassword.CommunicationsAddress == sourceAddress;
	}

	public bool TryChangeManualAcknowledgementTimeout(
		ManualAcknowledgementTimeout manualAcknowledgementTimeout,
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		ArgumentNullException.ThrowIfNull(manualAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(requiredPasswordLevel);
		ArgumentNullException.ThrowIfNull(sourceAddress);

		while (true)
		{
			var current = this.GetCurrent();
			if (!IsAuthorized(current, requiredPasswordLevel, sourceAddress))
			{
				return false;
			}

			var changed = current.WithManualAcknowledgementTimeout(manualAcknowledgementTimeout);
			if (ReferenceEquals(
				Interlocked.CompareExchange(ref this.currentParameters, changed, current),
				current))
			{
				return true;
			}
		}
	}

	public bool TryChangeNoAcknowledgementTimeout(
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		PasswordLevel requiredPasswordLevel,
		CommunicationsAddress sourceAddress)
	{
		ArgumentNullException.ThrowIfNull(noAcknowledgementTimeout);
		ArgumentNullException.ThrowIfNull(requiredPasswordLevel);
		ArgumentNullException.ThrowIfNull(sourceAddress);

		while (true)
		{
			var current = this.GetCurrent();
			if (!IsAuthorized(current, requiredPasswordLevel, sourceAddress))
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
