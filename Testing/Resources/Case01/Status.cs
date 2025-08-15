using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Resources.Case01;

public class Status : BaseTable
{
	public string Name { get; set; } = default!;
	public string? Description { get; set; }

	public ICollection<Order> Orders { get; set; } = [];
}

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
	public void Configure(EntityTypeBuilder<Status> builder)
	{		
		builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
		builder.Property(s => s.Description).HasMaxLength(255);		
	}
}
