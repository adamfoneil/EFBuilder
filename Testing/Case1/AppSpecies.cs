using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Testing.Case1;

public class AppSpecies
{
	public int Id { get; set; }
	public string Name { get; set; } = default!;

	public ICollection<Species> Species { get; set; } = [];
	public ICollection<Breed> Breeds { get; set; } = [];
}

public class AppSpeciesConfiguration : IEntityTypeConfiguration<AppSpecies>
{
	public void Configure(EntityTypeBuilder<AppSpecies> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
	}
}