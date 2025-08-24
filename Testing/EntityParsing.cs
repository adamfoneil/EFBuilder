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
			// Use whitespace-tolerant comparison for the test result
			var normalizedActual = NormalizeContentForComparison(actualContent);
			var normalizedExpected = NormalizeContentForComparison(expectedContent);
			var contentsMatch = normalizedActual == normalizedExpected;
			
			if (actualContent != expectedContent)
			{
				if (!contentsMatch)
				{
					failedFiles.Add(file);
				}
				debugInfo.AppendLine($"\n=== DETAILED COMPARISON FOR {file} ===");
				debugInfo.AppendLine($"Exact match: {actualContent == expectedContent}");
				debugInfo.AppendLine($"Normalized match: {contentsMatch}");
				
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

	[TestMethod]
	public void Case2Schema()
	{
		if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");

		var schema = new ResourceEnumerator("Testing.SpayWise.", [
			"Location.md",
			"Appointment.md",
			"AppointmentType.md",
			"Client.md",
			"VolumeClient.md"
		]);
	}

	[TestMethod]
	public void AutoIncrement()
	{
		if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");

		var schema = new ResourceEnumerator("Testing.AutoIncrement.", [
			"AspNetUsers.md"
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
			foreach (var prop in entity.Properties)
			{
				debugInfo.AppendLine($"  Property: {prop.Name}, Type: {prop.ClrType}, IsAutoIncrement: {prop.IsAutoIncrement}, IsUnique: {prop.IsUnique}");
			}
		}

		var settings = new CodeGenerator.Settings()
		{
			DefaultNamespace = "Generated"
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
			"AspNetUsers.cs"
		];

		var expectedFiles = new ResourceEnumerator("Testing.AutoIncrement.", outputFiles);

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
			var expectedKey = $"Testing.AutoIncrement.{file}";
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
			// Use whitespace-tolerant comparison for the test result
			var normalizedActual = NormalizeContentForComparison(actualContent);
			var normalizedExpected = NormalizeContentForComparison(expectedContent);
			var contentsMatch = normalizedActual == normalizedExpected;
			
			if (actualContent != expectedContent)
			{
				if (!contentsMatch)
				{
					failedFiles.Add(file);
				}
				debugInfo.AppendLine($"\n=== DETAILED COMPARISON FOR {file} ===");
				debugInfo.AppendLine($"Exact match: {actualContent == expectedContent}");
				debugInfo.AppendLine($"Normalized match: {contentsMatch}");
				
				// Create unified diff format
				var diffOutput = CreateUnifiedDiff(expectedContent, actualContent, $"expected_{file}", $"actual_{file}");
				debugInfo.AppendLine("UNIFIED DIFF:");
				debugInfo.AppendLine(diffOutput);
				
				// Save diff to separate file
				System.IO.File.WriteAllText($"tmp/diff_{file}.txt", diffOutput);
				
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
				debugInfo.AppendLine($"  tmp/expected_{file} vs tmp/actual_{file}");
			}
			
			System.IO.File.WriteAllText("tmp/debug_output.txt", debugInfo.ToString());
			Assert.Fail($"Files did not match: {string.Join(", ", failedFiles)}. Debug info written to /tmp/debug_output.txt");
		}
		
		Assert.AreEqual(outputFiles.Length, actualFiles.Length, "Number of generated files doesn't match expected");
	}

	[TestMethod]
	public void SqlServerScaffolderTypeMappingTest()
	{
		var scaffolder = new SqlServerScaffolder();
		var typeMapping = scaffolder.GetTypeMapping();

		// Test basic CLR type mappings
		Assert.AreEqual("string", typeMapping["varchar"]);
		Assert.AreEqual("string", typeMapping["nvarchar"]);
		Assert.AreEqual("int", typeMapping["int"]);
		Assert.AreEqual("long", typeMapping["bigint"]);
		Assert.AreEqual("bool", typeMapping["bit"]);
		Assert.AreEqual("DateTime", typeMapping["datetime"]);
		Assert.AreEqual("DateOnly", typeMapping["date"]);
		Assert.AreEqual("TimeOnly", typeMapping["time"]);
		Assert.AreEqual("decimal", typeMapping["decimal"]);
		Assert.AreEqual("Guid", typeMapping["uniqueidentifier"]);
		Assert.AreEqual("byte[]", typeMapping["varbinary"]);

		// Test case insensitivity using TryGetValue
		Assert.IsTrue(typeMapping.TryGetValue("VARCHAR", out var varcharType));
		Assert.AreEqual("string", varcharType);
		Assert.IsTrue(typeMapping.TryGetValue("INT", out var intType));
		Assert.AreEqual("int", intType);

		// Test that we have a reasonable number of mappings - check actual count first
		Assert.IsTrue(typeMapping.Count >= 15, $"Expected at least 15 type mappings, got {typeMapping.Count}");
		
		// Verify we have the core SQL Server types
		var expectedTypes = new[] { "varchar", "nvarchar", "int", "bigint", "bit", "datetime", "decimal", "uniqueidentifier" };
		foreach (var type in expectedTypes)
		{
			Assert.IsTrue(typeMapping.ContainsKey(type), $"Missing type mapping for {type}");
		}
	}

	[TestMethod]
	public void SqlServerScaffolderInstantiationTest()
	{
		// Test that SqlServerScaffolder can be instantiated without errors
		var scaffolder = new SqlServerScaffolder();
		Assert.IsNotNull(scaffolder);
		
		// Test that it implements IScaffolder interface
		Assert.IsInstanceOfType(scaffolder, typeof(IScaffolder));
		
		// Test that GetTypeMapping returns valid mappings
		var typeMapping = scaffolder.GetTypeMapping();
		Assert.IsNotNull(typeMapping);
		Assert.IsTrue(typeMapping.Count > 0);
		
		// Verify the interface contract
		Assert.IsNotNull(typeMapping);
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
	
	/// <summary>
	/// Normalize content for comparison by removing trailing whitespace, 
	/// normalizing line endings, and removing empty lines
	/// </summary>
	private static string NormalizeContentForComparison(string content)
	{
		if (string.IsNullOrEmpty(content))
			return string.Empty;
			
		var lines = content.Split('\n')
			.Select(line => line.TrimEnd()) // Remove trailing whitespace including \r
			.Where(line => !string.IsNullOrEmpty(line)) // Remove empty lines
			.ToArray();
			
		return string.Join('\n', lines);
	}

	[TestMethod]
	public void ExplicitFK()
	{
		if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");

		var schema = new ResourceEnumerator("Testing.ExplicitFK.", [
			"Client.md",
			"Clinic.md",
			"Patient.md"
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
			foreach (var prop in entity.Properties)
			{
				debugInfo.AppendLine($"  Property: {prop.Name}, Type: {prop.ClrType}, ReferencedEntity: {prop.ReferencedEntity ?? "None"}, ChildCollection: {prop.ChildCollection ?? "None"}, IsNullable: {prop.IsNullable}");
			}
		}

		var settings = new CodeGenerator.Settings()
		{
			DefaultNamespace = "Generated",
			BaseClassNamespace = "Testing.Conventions"
		};

		var actualFiles = CodeGenerator.Execute(settings, definitions);
		
		// Debug output to file
		debugInfo.AppendLine($"Generated {actualFiles.Length} files:");
		foreach (var file in actualFiles)
		{
			debugInfo.AppendLine($"  {file.Filename}");
		}
		
		System.IO.File.WriteAllText("tmp/debug_explicit_fk.txt", debugInfo.ToString());
		
		var actualOutputByName = actualFiles.ToDictionary(f => f.Filename);

		string[] outputFiles = [
			"Client.cs",
			"Clinic.cs",
			"Patient.cs"
		];

		var expectedFiles = new ResourceEnumerator("Testing.ExplicitFK.", outputFiles);

		var expectedOutput = expectedFiles.GetContent();
		
		// Debug expected output
		debugInfo.AppendLine($"Expected output files:");
		foreach (var exp in expectedOutput)
		{
			debugInfo.AppendLine($"  Name: '{exp.Name}', Content length: {exp.Content.Length}");
		}
		
		System.IO.File.WriteAllText("tmp/debug_explicit_fk.txt", debugInfo.ToString());
		
		var expectedOutputByName = expectedOutput.ToDictionary(e => e.Name);
		var failedFiles = new List<string>();

		foreach (var file in outputFiles)
		{
			if (!actualOutputByName.ContainsKey(file))
			{
				Assert.Fail($"Missing generated file: {file}. Debug info written to tmp/debug_explicit_fk.txt");
			}
			
			// The expected output has the full resource name with namespace prefix
			var expectedKey = $"Testing.ExplicitFK.{file}";
			if (!expectedOutputByName.ContainsKey(expectedKey))
			{
				Assert.Fail($"Missing expected file: {expectedKey}. Available keys: {string.Join(", ", expectedOutputByName.Keys)}");
			}
			
			var actualContent = actualOutputByName[file].Content;
			var expectedContent = expectedOutputByName[expectedKey].Content;
			
			// Write actual content to file for debugging
			System.IO.File.WriteAllText($"tmp/actual_explicit_fk_{file}", actualContent);
			System.IO.File.WriteAllText($"tmp/expected_explicit_fk_{file}", expectedContent);
			
			// Enhanced diagnostic output
			// Use whitespace-tolerant comparison for the test result
			var normalizedActual = NormalizeContentForComparison(actualContent);
			var normalizedExpected = NormalizeContentForComparison(expectedContent);
			var contentsMatch = normalizedActual == normalizedExpected;
			
			if (actualContent != expectedContent)
			{
				if (!contentsMatch)
				{
					failedFiles.Add(file);
				}
				debugInfo.AppendLine($"\n=== DETAILED COMPARISON FOR {file} ===");
				debugInfo.AppendLine($"Exact match: {actualContent == expectedContent}");
				debugInfo.AppendLine($"Normalized match: {contentsMatch}");
				
				// Create unified diff format
				var diffOutput = CreateUnifiedDiff(expectedContent, actualContent, $"expected_{file}", $"actual_{file}");
				debugInfo.AppendLine("UNIFIED DIFF:");
				debugInfo.AppendLine(diffOutput);
				
				// Write diff to file for external examination
				System.IO.File.WriteAllText($"tmp/diff_explicit_fk_{file}.txt", diffOutput);
				
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
				
				Console.WriteLine($"✗ {file} differs - see tmp/debug_explicit_fk.txt and tmp/diff_explicit_fk_{file}.txt for details");
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
				debugInfo.AppendLine($"  tmp/expected_explicit_fk_{file} vs tmp/actual_explicit_fk_{file}");
			}
		}
		
		System.IO.File.WriteAllText("tmp/debug_explicit_fk.txt", debugInfo.ToString());
		
		if (failedFiles.Any())
		{
			Assert.Fail($"Content differs for {failedFiles.Count} file(s): {string.Join(", ", failedFiles)}. See detailed analysis in tmp/debug_explicit_fk.txt");
		}
	}

	[TestMethod]
	public void CustomPrimaryReference()
	{
		if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");

		var schema = new ResourceEnumerator("Testing.CustomPrimaryRef.", [
			"AspNetUsers.md",
			"ClinicUser.md"
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
			foreach (var prop in entity.Properties)
			{
				debugInfo.AppendLine($"  Property: {prop.Name}, Type: {prop.ClrType}, ReferencedEntity: {prop.ReferencedEntity ?? "None"}, ReferencedColumn: {prop.ReferencedColumn ?? "None"}, IsUnique: {prop.IsUnique}, IsAutoIncrement: {prop.IsAutoIncrement}");
			}
		}

		var settings = new CodeGenerator.Settings()
		{
			DefaultNamespace = "Generated"
		};

		var actualFiles = CodeGenerator.Execute(settings, definitions);
		
		// Debug output to file
		debugInfo.AppendLine($"Generated {actualFiles.Length} files:");
		foreach (var file in actualFiles)
		{
			debugInfo.AppendLine($"  {file.Filename}");
		}
		
		System.IO.File.WriteAllText("tmp/debug_custom_ref.txt", debugInfo.ToString());
		
		var actualOutputByName = actualFiles.ToDictionary(f => f.Filename);

		string[] outputFiles = [
			"AspNetUsers.cs",
			"ClinicUser.cs"
		];

		var expectedFiles = new ResourceEnumerator("Testing.CustomPrimaryRef.", outputFiles);

		var expectedOutput = expectedFiles.GetContent();
		
		// Debug expected output
		debugInfo.AppendLine($"Expected output files:");
		foreach (var exp in expectedOutput)
		{
			debugInfo.AppendLine($"  Name: '{exp.Name}', Content length: {exp.Content.Length}");
		}
		
		System.IO.File.WriteAllText("tmp/debug_custom_ref.txt", debugInfo.ToString());
		
		var expectedOutputByName = expectedOutput.ToDictionary(e => e.Name);
		var failedFiles = new List<string>();

		foreach (var file in outputFiles)
		{
			if (!actualOutputByName.ContainsKey(file))
			{
				Assert.Fail($"Missing generated file: {file}. Debug info written to tmp/debug_custom_ref.txt");
			}
			
			// The expected output has the full resource name with namespace prefix
			var expectedKey = $"Testing.CustomPrimaryRef.{file}";
			if (!expectedOutputByName.ContainsKey(expectedKey))
			{
				Assert.Fail($"Missing expected file: {expectedKey}. Available keys: {string.Join(", ", expectedOutputByName.Keys)}");
			}
			
			var actualContent = actualOutputByName[file].Content;
			var expectedContent = expectedOutputByName[expectedKey].Content;
			
			// Write actual content to file for debugging
			System.IO.File.WriteAllText($"tmp/actual_custom_ref_{file}", actualContent);
			System.IO.File.WriteAllText($"tmp/expected_custom_ref_{file}", expectedContent);
			
			// Enhanced diagnostic output
			// Use whitespace-tolerant comparison for the test result
			var normalizedActual = NormalizeContentForComparison(actualContent);
			var normalizedExpected = NormalizeContentForComparison(expectedContent);
			var contentsMatch = normalizedActual == normalizedExpected;
			
			if (actualContent != expectedContent)
			{
				if (!contentsMatch)
				{
					failedFiles.Add(file);
				}
				debugInfo.AppendLine($"\n=== DETAILED COMPARISON FOR {file} ===");
				debugInfo.AppendLine($"Exact match: {actualContent == expectedContent}");
				debugInfo.AppendLine($"Normalized match: {contentsMatch}");
				
				// Create unified diff format
				var diffOutput = CreateUnifiedDiff(expectedContent, actualContent, $"expected_{file}", $"actual_{file}");
				debugInfo.AppendLine("UNIFIED DIFF:");
				debugInfo.AppendLine(diffOutput);
				
				// Write diff to file for external examination
				System.IO.File.WriteAllText($"tmp/diff_custom_ref_{file}.txt", diffOutput);
				
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
				
				Console.WriteLine($"✗ {file} differs - see tmp/debug_custom_ref.txt and tmp/diff_custom_ref_{file}.txt for details");
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
				debugInfo.AppendLine($"  tmp/expected_custom_ref_{file} vs tmp/actual_custom_ref_{file}");
			}
		}
		
		System.IO.File.WriteAllText("tmp/debug_custom_ref.txt", debugInfo.ToString());
		
		if (failedFiles.Any())
		{
			Assert.Fail($"Content differs for {failedFiles.Count} file(s): {string.Join(", ", failedFiles)}. See detailed analysis in tmp/debug_custom_ref.txt");
		}
	}
}
