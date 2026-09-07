using Microsoft.EntityFrameworkCore;

namespace Router.Persistence;

public sealed class RouterDbContext : DbContext
{
	internal DbSet<CommunicationsNodeRecord> CommunicationsNodes => this.Set<CommunicationsNodeRecord>();
	internal DbSet<ManagedEntityRecord> ManagedEntities => this.Set<ManagedEntityRecord>();
	internal DbSet<ParameterSetRecord> ParameterSets => this.Set<ParameterSetRecord>();
	internal DbSet<PersistedParameterValueRecord> ParameterValues => this.Set<PersistedParameterValueRecord>();
	internal DbSet<PasswordVerifierRecord> PasswordVerifiers => this.Set<PasswordVerifierRecord>();

	public RouterDbContext(DbContextOptions<RouterDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("node");

		modelBuilder.Entity<CommunicationsNodeRecord>(entity =>
		{
			entity.ToTable("communications_node");
			entity.HasKey(node => node.Id);
		});

		modelBuilder.Entity<ManagedEntityRecord>(entity =>
		{
			entity.ToTable("managed_entity");
			entity.HasKey(managedEntity => managedEntity.Id);
			entity.HasIndex(managedEntity => new { managedEntity.NodeId, managedEntity.Kind }).IsUnique();
			entity.HasIndex(managedEntity => managedEntity.Kind).IsUnique();
			entity.HasOne<CommunicationsNodeRecord>()
				.WithMany()
				.HasForeignKey(managedEntity => managedEntity.NodeId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<ParameterSetRecord>(entity =>
		{
			entity.ToTable("parameter_set");
			entity.HasKey(parameterSet => parameterSet.Id);
			entity.HasIndex(parameterSet => new { parameterSet.ManagedEntityId, parameterSet.Kind }).IsUnique();
			entity.Property(parameterSet => parameterSet.Revision).IsConcurrencyToken();
			entity.HasOne<ManagedEntityRecord>()
				.WithMany()
				.HasForeignKey(parameterSet => parameterSet.ManagedEntityId)
				.OnDelete(DeleteBehavior.Cascade);
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

		modelBuilder.Entity<PasswordVerifierRecord>(entity =>
		{
			entity.ToTable(
				"password_verifier",
				"security",
				table => table.HasCheckConstraint(
					"CK_password_verifier_ParameterNumber",
					"\"ParameterNumber\" = 5"));
			entity.HasKey(passwordVerifier => new
			{
				passwordVerifier.ParameterSetId,
				passwordVerifier.ParameterNumber
			});
			entity.Property(passwordVerifier => passwordVerifier.Salt).HasColumnType("bytea");
			entity.Property(passwordVerifier => passwordVerifier.Hash).HasColumnType("bytea");
			entity.HasOne<ParameterSetRecord>()
				.WithMany()
				.HasForeignKey(passwordVerifier => passwordVerifier.ParameterSetId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}

internal sealed class CommunicationsNodeRecord
{
	public Guid Id { get; set; }
}

internal sealed class ManagedEntityRecord
{
	public Guid Id { get; set; }
	public Guid NodeId { get; set; }
	public ManagedEntityKind Kind { get; set; }
}

internal enum ManagedEntityKind : byte
{
	Router = 0
}

internal sealed class ParameterSetRecord
{
	public Guid Id { get; set; }
	public Guid ManagedEntityId { get; set; }
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

internal sealed class PasswordVerifierRecord
{
	public Guid ParameterSetId { get; set; }
	public byte ParameterNumber { get; set; }
	public int Version { get; set; }
	public int WorkFactor { get; set; }
	public byte[] Salt { get; set; } = [];
	public byte[] Hash { get; set; } = [];
}
