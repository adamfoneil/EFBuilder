using ModelBuilder;

namespace Testing;

[TestClass]
public class EntityParsing
{
	[TestMethod]
	public void Case1Schema()
	{
		var schema = new ResourceEnumerator("Testing.SpayWise.", [
			"Clinic.md",
			"Species.md",
			"Breed.md",
			"AppSpecies.md"
		]);
			
		var (definitions, errors) = new EntityParser(schema).ParseEntities();

		// Debug output to file
		var debugInfo = new System.Text.StringBuilder();
		debugInfo.AppendLine($"Found {definitions.Length} entities, {errors.Length} errors");
		foreach (var error in errors)
		{
			debugInfo.AppendLine($"Error: {error}");
		}
		foreach (var entity in definitions)
		{
			debugInfo.AppendLine($"Entity: {entity.Name}, BaseClass: {entity.BaseClass ?? "None"}");
		}

		var settings = new CodeGenerator.Settings()
		{
			DefaultNamespace = "Testing.Case1",
			BaseClassNamespace = "Testing.Conventions"
		};

		var actualFiles = CodeGenerator.Execute(settings, definitions);
		
		// Debug output to file
		debugInfo.AppendLine($"Generated {actualFiles.Length} files:");
		foreach (var file in actualFiles)
		{
			debugInfo.AppendLine($"  {file.Filename}");
		}
		
		System.IO.File.WriteAllText("/tmp/debug_output.txt", debugInfo.ToString());
		
		var actualOutputByName = actualFiles.ToDictionary(f => f.Filename);

		string[] outputFiles = [
			"Clinic.cs",
			"Species.cs",
			"Breed.cs",
			"AppSpecies.cs"
		];

		var expectedFiles = new ResourceEnumerator("Testing.Case1.", outputFiles);

		var expectedOutput = expectedFiles.GetContent();
		
		// Debug expected output
		debugInfo.AppendLine($"Expected output files:");
		foreach (var exp in expectedOutput)
		{
			debugInfo.AppendLine($"  Name: '{exp.Name}', Content length: {exp.Content.Length}");
		}
		
		System.IO.File.WriteAllText("/tmp/debug_output.txt", debugInfo.ToString());
		
		var expectedOutputByName = expectedOutput.ToDictionary(e => e.Name);

		foreach (var file in outputFiles)
		{
			if (!actualOutputByName.ContainsKey(file))
			{
				Assert.Fail($"Missing generated file: {file}. Debug info written to /tmp/debug_output.txt");
			}
			
			// The expected output has the full resource name with namespace prefix
			var expectedKey = $"Testing.Case1.{file}";
			if (!expectedOutputByName.ContainsKey(expectedKey))
			{
				Assert.Fail($"Missing expected file: {expectedKey}. Available keys: {string.Join(", ", expectedOutputByName.Keys)}");
			}
			
			Assert.AreEqual(expectedOutputByName[expectedKey].Content, actualOutputByName[file].Content, $"File: {file}");
		}
	}
}
