using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case1;

public class Species : BaseTable
{
	public int ClinicId { get; set; }
	public string Name { get; set; } = default!;
	public int AppSpeciesId { get; set; }
	public string BaseName { get; set; } = default!;
	public string Abbreviation { get; set; } = default!;
	public int? MinWeight { get; set; }
	public bool IsActive { get; set; } = true;

	public Clinic? Clinic { get; set; }
	public AppSpecies? AppSpecies { get; set; }
}

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
	public void Configure(EntityTypeBuilder<Species> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
		builder.Property(x => x.BaseName).IsRequired().HasMaxLength(50);
		builder.Property(x => x.Abbreviation).IsRequired().HasMaxLength(3);

		builder.HasIndex(e => new { e.ClinicId, e.Name }).IsUnique();

		builder.HasOne(e => e.Clinic).WithMany(e => e.Species).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);		
		builder.HasOne(e => e.AppSpecies).WithMany(e => e.Species).HasForeignKey(x => x.AppSpeciesId).OnDelete(DeleteBehavior.Restrict);
	}
}
