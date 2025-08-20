namespace ModelBuilder;

/// <summary>
/// something that can extract entity definitions from a source
/// </summary>
public interface IContentAccessor
{
	string[] GetSources();
}
