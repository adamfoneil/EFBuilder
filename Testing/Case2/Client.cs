using Testing.Case1;
using Testing.Conventions;

namespace Testing.Case2;

public class Client : BaseTable
{
	public int ClinicId { get; set; }
	public string Name { get; set; } = default!;

	public Clinic? Clinic { get; set; }
}
