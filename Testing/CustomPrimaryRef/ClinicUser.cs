using Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Generated;

public class ClinicUser : BaseTable
{
	public int ClinicId { get; set; }
	public int UserId { get; set; }
	public bool IsEnabled { get; set; }
	public long Permissions { get; set; }

	public Clinic? Clinic { get; set; }
	public AspNetUsers? User { get; set; }
}

public class ClinicUserConfiguration : IEntityTypeConfiguration<ClinicUser>
{
	public void Configure(EntityTypeBuilder<ClinicUser> builder)
	{
		builder.HasIndex(e => new { e.ClinicId, e.UserId }).IsUnique();

		builder.HasOne(e => e.Clinic).WithMany(e => e.ClinicUsers).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
		builder.HasOne(e => e.User).WithMany(e => e.ClinicUsers).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
	}
}
