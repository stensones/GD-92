using Microsoft.EntityFrameworkCore;

namespace PrinterUA.Persistence;

public sealed class PrinterUaDbContext : DbContext
{
	internal DbSet<ParameterSetRecord> ParameterSets => this.Set<ParameterSetRecord>();
	internal DbSet<PersistedParameterValueRecord> ParameterValues => this.Set<PersistedParameterValueRecord>();

	public PrinterUaDbContext(DbContextOptions<PrinterUaDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("node");

		modelBuilder.Entity<ParameterSetRecord>(entity =>
		{
			entity.ToTable("parameter_set");
			entity.HasKey(parameterSet => parameterSet.Id);
			entity.HasIndex(parameterSet => parameterSet.Kind).IsUnique();
			entity.Property(parameterSet => parameterSet.Revision).IsConcurrencyToken();
		});

		modelBuilder.Entity<PersistedParameterValueRecord>(entity =>
		{
			entity.ToTable("parameter_value");
			entity.HasKey(parameterValue => new
			{
				parameterValue.ParameterSetId,
				parameterValue.ParameterNumber
			});
			entity.Property(parameterValue => parameterValue.EncodedValue).HasColumnType("bytea");
			entity.HasOne<ParameterSetRecord>()
				.WithMany()
				.HasForeignKey(parameterValue => parameterValue.ParameterSetId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}

internal sealed class ParameterSetRecord
{
	public Guid Id { get; set; }
	public PersistedParameterTableKind Kind { get; set; }
	public long Revision { get; set; }
}

internal enum PersistedParameterTableKind : byte
{
	Permanent = 0,
	NonVolatile = 1
}

internal sealed class PersistedParameterValueRecord
{
	public Guid ParameterSetId { get; set; }
	public byte ParameterNumber { get; set; }
	public byte[] EncodedValue { get; set; } = [];
}
