namespace Router.Persistence;

public sealed class RouterCurrentParameterProjectionSource
{
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
}
