namespace NodeManager.Persistence;

public sealed class NodeManagerCurrentParameterProjectionSource
{
	private NodeManagerCurrentParameterProjection? currentParameters;

	public void Publish(NodeManagerCurrentParameterProjection projection)
	{
		ArgumentNullException.ThrowIfNull(projection);
		Interlocked.Exchange(ref this.currentParameters, projection);
	}

	public NodeManagerCurrentParameterProjection GetCurrent()
	{
		return Volatile.Read(ref this.currentParameters) ??
			throw new InvalidOperationException("NodeManager current Parameters have not been loaded.");
	}
}
