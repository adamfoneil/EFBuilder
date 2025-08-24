using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case2;

public class Location : BaseTable
{
	public int ClinicId { get; set; }
	public string Name { get; set; } = default!;
	public string Address { get; set; } = default!;
	public string City { get; set; } = default!;
	public string State { get; set; } = default!;
	public string ZipCode { get; set; } = default!;
	public string Phone { get; set; } = default!;
	public string? Email { get; set; }
	public bool IsActive { get; set; } = true;

	public Clinic? Clinic { get; set; }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
	public void Configure(EntityTypeBuilder<Location> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
		builder.Property(x => x.Address).IsRequired().HasMaxLength(100);
		builder.Property(x => x.City).IsRequired().HasMaxLength(50);
		builder.Property(x => x.State).IsRequired().HasMaxLength(2);
		builder.Property(x => x.ZipCode).IsRequired().HasMaxLength(10);
		builder.Property(x => x.Phone).IsRequired().HasMaxLength(20);
		builder.Property(x => x.Email).HasMaxLength(50);
		builder.HasIndex(e => new { e.ClinicId, e.Name }).IsUnique();
	}
}
