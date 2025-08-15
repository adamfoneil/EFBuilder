using System.ComponentModel.DataAnnotations;

namespace Testing.Conventions;

public class BaseTable
{
	public int Id { get; set; }

	[MaxLength(50)]
	public string CreatedBy { get; set; } = "system";
	public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;

	[MaxLength(50)]
	public string? UpdatedBy { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
}