using ModelBuilder;
using System.Reflection;

namespace Testing;

internal class ResourceEnumerator(string prefix, IEnumerable<string> fileNames) : IEntityEnumerator
{
	private readonly string _prefix = prefix;
	private readonly IEnumerable<string> _fileNames = fileNames;

	public (string Name, string Content)[] GetContent()
	{
		var resourceNames = Assembly.GetExecutingAssembly()
			.GetManifestResourceNames()
			.Join(_fileNames, name => name, name => $"{_prefix}.{name}", (resourceName, fileName) => resourceName, StringComparer.OrdinalIgnoreCase);

		List<(string Name, string Content)> results = [];

		foreach (var fileName in resourceNames)
		{
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName) ?? throw new Exception("resource not found");
			using var reader = new StreamReader(stream);
			string content = reader.ReadToEnd();
			results.Add((fileName, content));
		}

		return [..results];
	}
}
