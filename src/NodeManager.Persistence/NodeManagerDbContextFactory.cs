using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NodeManager.Persistence;

public sealed class NodeManagerDbContextFactory : IDesignTimeDbContextFactory<NodeManagerDbContext>
{
	public NodeManagerDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<NodeManagerDbContext>()
			.UseNpgsql("Host=localhost;Database=node-manager;Username=postgres;Password=postgres")
			.Options;

		return new NodeManagerDbContext(options);
	}
}
