namespace ModelBuilder;

/// <summary>
/// something that can extract entity definitions from a source (files in a directory, rows in a database, embedded resources)
/// </summary>
public interface IEntityEnumerator
{
	/// <summary>
	/// get the raw markdown for each entity in a source
	/// </summary>	
	(string Name, string Content)[] GetEntitySources();
}
