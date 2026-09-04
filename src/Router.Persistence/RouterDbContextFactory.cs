using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Router.Persistence;

public sealed class RouterDbContextFactory : IDesignTimeDbContextFactory<RouterDbContext>
{
	public RouterDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql("Host=localhost;Database=router;Username=postgres;Password=postgres")
			.Options;

		return new RouterDbContext(options);
	}
}
