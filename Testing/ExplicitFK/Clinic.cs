using Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Generated;

public class Clinic
{
	public string Name { get; set; } = default!;
	public string Address { get; set; } = default!;
	public string City { get; set; } = default!;
	public string State { get; set; } = default!;
	public string ZipCode { get; set; } = default!;
	public string PrimaryPhone { get; set; } = default!;
	public string? EmergencyPhone { get; set; }

	public ICollection<Client> Clients { get; set; } = [];	
	public ICollection<Patient> Patients { get; set; } = [];
	public ICollection<Species> Species { get; set; } = [];
}

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
	public void Configure(EntityTypeBuilder<Clinic> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
		builder.Property(x => x.Address).IsRequired().HasMaxLength(50);
		builder.Property(x => x.City).IsRequired().HasMaxLength(50);
		builder.Property(x => x.State).IsRequired().HasMaxLength(2);
		builder.Property(x => x.ZipCode).IsRequired().HasMaxLength(10);
		builder.Property(x => x.PrimaryPhone).IsRequired().HasMaxLength(20);
		builder.Property(e => e.EmergencyPhone).HasMaxLength(20);
	}
}
