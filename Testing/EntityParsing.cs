using ModelBuilder;

namespace Testing;

[TestClass]
public class EntityParsing
{
	[TestMethod]
	public void Species()
	{
		var schema = new ResourceEnumerator("Testing.SpayWise.", [
			"Clinic.md",
			"Species.md",
			"Breed.md",
			"AppSpecies.md"
		]);
			
		var (definitions, _) = new EntityParser(schema).ParseEntities();

		var settings = new CodeGenerator.Settings()
		{
			DefaultNamespace = "Testing.Case1",
			BaseClassNamespace = "Testing.Conventions"
		};

		var actualFiles = CodeGenerator.Execute(settings, definitions);
		var actualOutputByName = actualFiles.ToDictionary(f => f.Filename);

		string[] outputFiles = [
			"Clinic.cs",
			"Species.cs",
			"Breed.cs",
			"AppSpecies.cs"
		];

		var expectedFiles = new ResourceEnumerator("Testing.Case1.", outputFiles);

		var expectedOutput = expectedFiles.GetContent();
		var expectedOutputByName = expectedOutput.ToDictionary(e => e.Name);

		foreach (var file in outputFiles)
		{
			Assert.AreEqual(expectedOutputByName[file].Content, actualOutputByName[file].Content, $"File: {file}");
		}
	}
}
