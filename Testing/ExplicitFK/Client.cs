using Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generated;

public class Client
{
	public int ClinicId { get; set; }
	public string Name { get; set; } = default!;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }
	public string? State { get; set; }
	public string? ZipCode { get; set; }
	public string? PrimaryPhone { get; set; }
	public string? EmergencyPhone { get; set; }

	public Clinic? Clinic { get; set; }
	public ICollection<Patient> OwnerPatients { get; set; } = [];
	public ICollection<Patient> VolumePatients { get; set; } = [];
}

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
	public void Configure(EntityTypeBuilder<Client> builder)
	{
		builder.Property(x => x.Name).IsRequired();
		builder.Property(e => e.FirstName).HasMaxLength(50);
		builder.Property(e => e.LastName).HasMaxLength(50);
		builder.Property(e => e.Address).HasMaxLength(50);
		builder.Property(e => e.City).HasMaxLength(50);
		builder.Property(e => e.State).HasMaxLength(2);
		builder.Property(e => e.ZipCode).HasMaxLength(10);
		builder.Property(e => e.PrimaryPhone).HasMaxLength(20);
		builder.Property(e => e.EmergencyPhone).HasMaxLength(20);

		builder.HasOne(e => e.Clinic).WithMany(e => e.Clients).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
	}
}
