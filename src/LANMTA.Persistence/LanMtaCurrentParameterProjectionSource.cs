namespace LANMTA.Persistence;

public sealed class LanMtaCurrentParameterProjectionSource
{
	private LanMtaCurrentParameterProjection? currentParameters;

	public void Publish(LanMtaCurrentParameterProjection projection)
	{
		ArgumentNullException.ThrowIfNull(projection);
		Interlocked.Exchange(ref this.currentParameters, projection);
	}

	public LanMtaCurrentParameterProjection GetCurrent()
	{
		return Volatile.Read(ref this.currentParameters) ??
			throw new InvalidOperationException("LAN MTA current Parameters have not been loaded.");
	}
}
