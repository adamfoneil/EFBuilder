using ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Testing;

internal class ResourceEntityEnumerator(IEnumerable<string> fileNames) : IEntityEnumerator
{
	private readonly IEnumerable<string> _fileNames = fileNames;

	public (string Name, string Content)[] GetEntitySources()
	{
		var resourceNames = Assembly.GetExecutingAssembly()
			.GetManifestResourceNames()
			.Join(_fileNames, name => name, name => name.Split('.').Last(), (resourceName, fileName) => resourceName, StringComparer.OrdinalIgnoreCase);

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
