using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Generated;

public class Order : BaseTable
{
	public int CustomerId { get; set; };
	public DateTime Date { get; set; };
	public int StatusId { get; set; };
	public DateTime? StatusDate { get; set; };
	public decimal TotalAmount { get; set; };

	public Customer? Customer { get; set; }
	public Status? Status { get; set; }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
	public void Configure(EntityTypeBuilder<Order> builder)
	{

		builder.HasOne(o => o.Customer)
			.WithMany(e => e.Orders)
			.HasForeignKey(o => o.CustomerId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(o => o.Status)
			.WithMany(e => e.Orders)
			.HasForeignKey(o => o.StatusId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
