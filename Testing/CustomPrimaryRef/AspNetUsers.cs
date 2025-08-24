using Generated;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generated;

public class AspNetUsers : IdentityUser
{
	public int UserId { get; set; }

	public ICollection<ClinicUser> ClinicUsers { get; set; } = [];
}

public class AspNetUsersConfiguration : IEntityTypeConfiguration<AspNetUsers>
{
	public void Configure(EntityTypeBuilder<AspNetUsers> builder)
	{
		builder.Property(u => u.UserId).ValueGeneratedOnAdd();
		builder.HasIndex(e => e.UserId).IsUnique();
	}
}
