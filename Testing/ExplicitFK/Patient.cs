using Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Generated;

public class Patient : BaseTable
{
	public int ClinicId { get; set; }
	public int Number { get; set; }
	public string Name { get; set; } = default!;
	public int? OwnerClientId { get; set; }
	public int? VolumeClientId { get; set; }

	public Clinic? Clinic { get; set; }
	public Client? OwnerClient { get; set; }
	public Client? VolumeClient { get; set; }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
	public void Configure(EntityTypeBuilder<Patient> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
		builder.HasIndex(e => new { e.ClinicId, e.Number }).IsUnique();

		builder.HasOne(e => e.Clinic).WithMany(e => e.Patients).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
		builder.HasOne(e => e.OwnerClient).WithMany(e => e.OwnerPatients).HasForeignKey(x => x.OwnerClientId).OnDelete(DeleteBehavior.Restrict);
		builder.HasOne(e => e.VolumeClient).WithMany(e => e.VolumePatients).HasForeignKey(x => x.VolumeClientId).OnDelete(DeleteBehavior.Restrict);
	}
}
