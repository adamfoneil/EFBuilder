using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case1;

public class Clinic : BaseTable
{
	public string Name { get; set; } = default!;
	public bool IsActive { get; set; } = true;

	public ICollection<Species> Species { get; set; } = [];	
}

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
	public void Configure(EntityTypeBuilder<Clinic> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

		builder.HasIndex(e => e.Name).IsUnique().IsUnique();
	}
}