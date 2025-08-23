using ModelBuilder;
using System.Linq;

namespace Testing;

[TestClass]
public class EntityParsing
{
	[TestMethod]
	public void Case1Schema()
	{
		if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");

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
		
		System.IO.File.WriteAllText("tmp/debug_output.txt", debugInfo.ToString());
		
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
		
		System.IO.File.WriteAllText("tmp/debug_output.txt", debugInfo.ToString());
		
		var expectedOutputByName = expectedOutput.ToDictionary(e => e.Name);
		var failedFiles = new List<string>();

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
			
			var actualContent = actualOutputByName[file].Content;
			var expectedContent = expectedOutputByName[expectedKey].Content;
			
			// Write actual content to file for debugging
			System.IO.File.WriteAllText($"tmp/actual_{file}", actualContent);
			System.IO.File.WriteAllText($"tmp/expected_{file}", expectedContent);
			
			// Enhanced diagnostic output
			if (actualContent != expectedContent)
			{
				failedFiles.Add(file);
				debugInfo.AppendLine($"\n=== DETAILED COMPARISON FOR {file} ===");
				
				// Create unified diff format
				var diffOutput = CreateUnifiedDiff(expectedContent, actualContent, $"expected_{file}", $"actual_{file}");
				debugInfo.AppendLine("UNIFIED DIFF:");
				debugInfo.AppendLine(diffOutput);
				
				// Write diff to file for external examination
				System.IO.File.WriteAllText($"tmp/diff_{file}.txt", diffOutput);
				
				// Character-level analysis
				var charDiff = FindFirstDifference(expectedContent, actualContent);
				if (charDiff.HasValue)
				{
					debugInfo.AppendLine($"\nFirst difference at position {charDiff.Value.Position}:");
					debugInfo.AppendLine($"Expected: '{charDiff.Value.Expected}' (char code: {(int)charDiff.Value.Expected})");
					debugInfo.AppendLine($"Actual:   '{charDiff.Value.Actual}' (char code: {(int)charDiff.Value.Actual})");
				}
				
				// Line-by-line comparison
				debugInfo.AppendLine("\n=== LINE-BY-LINE COMPARISON ===");
				var lineComparison = CompareLines(expectedContent, actualContent);
				debugInfo.AppendLine(lineComparison);
				
				// Side-by-side comparison for easier reading
				debugInfo.AppendLine("=== SIDE-BY-SIDE COMPARISON ===");
				var sideBySide = CreateSideBySideComparison(expectedContent, actualContent);
				debugInfo.AppendLine(sideBySide);
				
				Console.WriteLine($"✗ {file} differs - see /tmp/debug_output.txt and /tmp/diff_{file}.txt for details");
			}
			else
			{
				Console.WriteLine($"✓ {file} matches exactly");
			}
		}
		
		// Write comprehensive debug info
		if (failedFiles.Any())
		{
			debugInfo.AppendLine($"\n=== SUMMARY ===");
			debugInfo.AppendLine($"Failed files: {string.Join(", ", failedFiles)}");
			debugInfo.AppendLine($"Passed files: {string.Join(", ", outputFiles.Except(failedFiles))}");
			debugInfo.AppendLine($"\nFor external diff tools, compare these file pairs:");
			foreach (var file in failedFiles)
			{
				debugInfo.AppendLine($"  /tmp/expected_{file} vs /tmp/actual_{file}");
			}
		}
		
		System.IO.File.WriteAllText("tmp/debug_output.txt", debugInfo.ToString());
		
		if (failedFiles.Any())
		{
			Assert.Fail($"Content differs for {failedFiles.Count} file(s): {string.Join(", ", failedFiles)}. See detailed analysis in /tmp/debug_output.txt");
		}
	}

	// Helper methods for enhanced diagnostics
	private static string CreateUnifiedDiff(string expected, string actual, string expectedFileName, string actualFileName)
	{
		var expectedLines = expected.Split('\n');
		var actualLines = actual.Split('\n');
		
		var diff = new System.Text.StringBuilder();
		diff.AppendLine($"--- {expectedFileName}");
		diff.AppendLine($"+++ {actualFileName}");
		
		int maxLines = Math.Max(expectedLines.Length, actualLines.Length);
		
		for (int i = 0; i < maxLines; i++)
		{
			var expectedLine = i < expectedLines.Length ? expectedLines[i] : "";
			var actualLine = i < actualLines.Length ? actualLines[i] : "";
			
			if (expectedLine != actualLine)
			{
				diff.AppendLine($"@@ -{i + 1} +{i + 1} @@");
				if (i < expectedLines.Length)
					diff.AppendLine($"-{expectedLine}");
				if (i < actualLines.Length)
					diff.AppendLine($"+{actualLine}");
			}
		}
		
		return diff.ToString();
	}
	
	private static (int Position, char Expected, char Actual)? FindFirstDifference(string expected, string actual)
	{
		int minLength = Math.Min(expected.Length, actual.Length);
		
		for (int i = 0; i < minLength; i++)
		{
			if (expected[i] != actual[i])
			{
				return (i, expected[i], actual[i]);
			}
		}
		
		// If all characters match but lengths differ
		if (expected.Length != actual.Length)
		{
			if (expected.Length > actual.Length)
				return (actual.Length, expected[actual.Length], '\0');
			else
				return (expected.Length, '\0', actual[expected.Length]);
		}
		
		return null;
	}
	
	private static string CompareLines(string expected, string actual)
	{
		var expectedLines = expected.Split('\n');
		var actualLines = actual.Split('\n');
		var result = new System.Text.StringBuilder();
		
		int maxLines = Math.Max(expectedLines.Length, actualLines.Length);
		
		for (int i = 0; i < maxLines; i++)
		{
			var expectedLine = i < expectedLines.Length ? expectedLines[i] : "<missing>";
			var actualLine = i < actualLines.Length ? actualLines[i] : "<missing>";
			
			if (expectedLine != actualLine)
			{
				result.AppendLine($"Line {i + 1}:");
				result.AppendLine($"  Expected: '{expectedLine.Replace('\r', '↵').Replace('\t', '→')}'");
				result.AppendLine($"  Actual:   '{actualLine.Replace('\r', '↵').Replace('\t', '→')}'");
				result.AppendLine();
			}
		}
		
		return result.ToString();
	}
	
	private static string CreateSideBySideComparison(string expected, string actual)
	{
		var expectedLines = expected.Split('\n');
		var actualLines = actual.Split('\n');
		var result = new System.Text.StringBuilder();
		
		result.AppendLine("Expected                                    | Actual");
		result.AppendLine("------------------------------------------- | -------------------------------------------");
		
		int maxLines = Math.Max(expectedLines.Length, actualLines.Length);
		
		for (int i = 0; i < maxLines; i++)
		{
			var expectedLine = i < expectedLines.Length ? expectedLines[i].Replace("\r", "").Replace("\t", "    ") : "";
			var actualLine = i < actualLines.Length ? actualLines[i].Replace("\r", "").Replace("\t", "    ") : "";
			
			// Truncate long lines for readability
			if (expectedLine.Length > 43) expectedLine = expectedLine.Substring(0, 40) + "...";
			if (actualLine.Length > 43) actualLine = actualLine.Substring(0, 40) + "...";
			
			var marker = expectedLine == actualLine ? " " : "*";
			result.AppendLine($"{expectedLine,-43} {marker} {actualLine}");
		}
		
		return result.ToString();
	}
}
