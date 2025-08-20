namespace ModelBuilder;

public class LocalSettings
{
	public string IdentityType { get; set; } = "int";
	public string DefaultNamespace { get; set; } = "Generated";
	public string? BaseClassNamespace { get; set; }
	/// <summary>
	/// DbContext type we can inspect to enable FK references/navigation properties
	/// </summary>
	public string? DbContextClass { get; set; }
}
