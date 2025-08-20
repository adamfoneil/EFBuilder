using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testing.Conventions;

public class BaseTable
{
	public int Id { get; set; }

	[MaxLength(50)]
	public string CreatedBy { get; set; } = "system";
	[Column(TypeName = "timestamp without time zone")]
	public DateTime CreatedAt { get; set; } = DateTime.Now;

	[MaxLength(50)]
	public string? UpdatedBy { get; set; }
	[Column(TypeName = "timestamp without time zone")]
	public DateTime? UpdatedAt { get; set; }
}