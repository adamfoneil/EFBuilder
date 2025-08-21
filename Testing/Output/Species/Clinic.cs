using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Output.Species;

public class Clinic : BaseTable
{
	public string Name { get; set; } = default!;
	public bool IsActive { get; set; } = true;
}

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
	public void Configure(EntityTypeBuilder<Clinic> builder)
	{		
		builder.Property(x => x.Name).IsRequired().HasMaxLength(100);	
	}
}