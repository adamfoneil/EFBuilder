using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case2;

public enum BuiltInAppointmentType
{
	SpayNeuter = 1,
	Wellness,
	Recheck
}

public class AppointmentType : BaseTable
{
	public int ClinicId { get; set; }
	public string Name { get; set; } = default!;
	public BuiltInAppointmentType? BuiltInType { get; set; }
	public string? BackColor { get; set; }
	public string? TextColor { get; set; }
	public bool IsActive { get; set; } = true;
}

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
{
	public void Configure(EntityTypeBuilder<AppointmentType> builder)
	{
		builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
		builder.Property(x => x.BackColor).HasMaxLength(20);
		builder.Property(x => x.TextColor).HasMaxLength(20);
		builder.HasIndex(e => new { e.ClinicId, e.Name }).IsUnique();
	}
}
