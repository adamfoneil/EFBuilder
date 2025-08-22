using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case1;

public class Breed : BaseTable
{
	public int AppSpeciesId { get; set; }
	public string Name { get; set; } = default!;

	public AppSpecies? AppSpecies { get; set; }
}

public class BreedConfiguration : IEntityTypeConfiguration<Breed>
{
	public void Configure(EntityTypeBuilder<Breed> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
		builder.HasIndex(e => e.Name).IsUnique().IsUnique();
		builder.HasOne(e => e.AppSpecies).WithMany(e => e.Breeds).HasForeignKey(x => x.AppSpeciesId).OnDelete(DeleteBehavior.Restrict);
	}
}
