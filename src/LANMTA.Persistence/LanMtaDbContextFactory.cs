using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LANMTA.Persistence;

public sealed class LanMtaDbContextFactory : IDesignTimeDbContextFactory<LanMtaDbContext>
{
	public LanMtaDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<LanMtaDbContext>()
			.UseNpgsql("Host=localhost;Database=lan-mta;Username=postgres;******")
			.Options;

		return new LanMtaDbContext(options);
	}
}
