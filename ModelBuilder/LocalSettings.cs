namespace ModelBuilder;

public class LocalSettings
{
	public string IdentityType { get; set; } = "int";
	/// <summary>
	/// for example SpayWise.Data
	/// </summary>
	public string DefaultNamespace { get; set; } = "Generated";
	/// <summary>
	/// for example SpayWise.Data.Conventions
	/// </summary>
	public string? BaseClassNamespace { get; set; }
	/// <summary>
	/// DbContext type we can inspect to enable FK references/navigation properties
	/// </summary>
	public string? DbContextClass { get; set; }
}
