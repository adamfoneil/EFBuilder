using System.Text;

namespace ModelBuilder;

public static class CodeGenerator
{
	public class Settings
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
	}

	public static (string Filename, string Content)[] Execute(Settings settings, EntityDefinition[] entities) =>
		[.. entities.Select(e => ($"{e.Name}.cs", Execute(settings, e, entities)))];

	public static string Execute(Settings settings, EntityDefinition entity, EntityDefinition[] allEntities)
	{
		StringBuilder sb = new();
		sb.AppendLine(
			$"""
			{GetRequiredUsingStatements(settings, entity)}using Microsoft.EntityFrameworkCore;
			using Microsoft.EntityFrameworkCore.Metadata.Builders;
			{UsingBaseClassNamespace(settings, entity)}

			namespace {settings.DefaultNamespace};

			""");

		sb.AppendLine(ClassDeclaration(entity));

		AddProperties(sb, entity);
		sb.AppendLine();
		AddParentNavProperties(sb, entity);
		AddChildCollections(sb, entity.Name, allEntities);

		sb.AppendLine("}");

		sb.AppendLine();

		sb.AppendLine($"public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>\n{{");
		sb.AppendLine($"\tpublic void Configure(EntityTypeBuilder<{entity.Name}> builder)\n\t{{");
		AddConfiguration(sb, entity);
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();

		static string ClassDeclaration(EntityDefinition e) => 
			$"public class {e.Name}" +
				(!string.IsNullOrWhiteSpace(e.BaseClass) ? $" : {e.BaseClass}" : string.Empty) + 
				"\n{";
	}

	private static void AddConfiguration(StringBuilder sb, EntityDefinition entity)
	{
		// Add required properties first
		foreach (var prop in entity.Properties.Where(p => p.ClrType == "string" && !p.IsNullable))
		{
			sb.AppendLine($"\t\tbuilder.Property(x => x.{prop.Name}).IsRequired(){(prop.MaxLength.HasValue ? $".HasMaxLength({prop.MaxLength.Value})" : "")};");
		}
		
		// Add max length for nullable string properties 
		foreach (var prop in entity.Properties.Where(p => p.ClrType == "string" && p.IsNullable && p.MaxLength.HasValue))
		{
			sb.AppendLine($"\t\tbuilder.Property(e => e.{prop.Name}).HasMaxLength({prop.MaxLength!.Value});");
		}

		// Add auto-increment properties
		foreach (var prop in entity.Properties.Where(p => p.IsAutoIncrement))
		{
			sb.AppendLine($"\t\tbuilder.Property(u => u.{prop.Name}).ValueGeneratedOnAdd();");
		}

		// Add unique indexes
		var uniqueProperties = entity.Properties.Where(p => p.IsUnique).ToArray();
		if (uniqueProperties.Any())
		{
			if (uniqueProperties.Length == 1)
			{
				sb.AppendLine($"\t\tbuilder.HasIndex(e => e.{uniqueProperties[0].Name}).IsUnique();");
			}
			else
			{
				// For multi-property indexes, use the actual property names
				var indexProperties = uniqueProperties.Select(p => $"e.{p.Name}");
				sb.AppendLine($"\t\tbuilder.HasIndex(e => new {{ {string.Join(", ", indexProperties)} }}).IsUnique();");
			}
		}

		// Add foreign key relationships
		var foreignKeys = entity.Properties.Where(p => !string.IsNullOrWhiteSpace(p.ReferencedEntity)).ToArray();
		if (foreignKeys.Any())
		{
			sb.AppendLine();
			for (int i = 0; i < foreignKeys.Length; i++)
			{
				var prop = foreignKeys[i];
				var navPropertyName = TrimIdEnding(prop.Name);
				var trailing = i == 0 ? "\t\t" : "";  // Only first foreign key has trailing spaces
				sb.AppendLine($"\t\tbuilder.HasOne(e => e.{navPropertyName}).WithMany(e => e.{prop.ChildCollection}).HasForeignKey(x => x.{prop.Name}).OnDelete(DeleteBehavior.Restrict);{trailing}");
			}
		}

		static string TrimIdEnding(string name) =>
			name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ? name[0..^2] : name;
	}

	private static void AddChildCollections(StringBuilder sb, string entityName, IEnumerable<EntityDefinition> allEntities)
	{
		var childCollections = allEntities
			.SelectMany(
				e => e.Properties.Where(p => p.ReferencedEntity?.Equals(entityName) ?? false && p.ChildCollection is not null),
				(e, p) => (e.Name, p.ChildCollection));
			
		foreach (var (childEntity, childCollection) in childCollections)
		{
			sb.AppendLine($"\tpublic ICollection<{childEntity}> {childCollection} {{ get; set; }} = [];\t");
		}
	}

	private static void AddParentNavProperties(StringBuilder sb, EntityDefinition entity)
	{
		foreach (var prop in entity.Properties.Where(p => !string.IsNullOrWhiteSpace(p.ReferencedEntity)))
		{
			sb.AppendLine($"\tpublic {prop.ReferencedEntity}? {TrimIdEnding(prop.Name)} {{ get; set; }}");
		}

		static string TrimIdEnding(string name) =>
			name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ? name[0..^2] : name;
	}

	private static void AddProperties(StringBuilder sb, EntityDefinition entity)
	{
		foreach (var prop in entity.Properties)
		{
			var defaultExpr = DefaultExpression(prop);
			var semicolon = string.IsNullOrEmpty(defaultExpr) ? "" : ";";
			sb.AppendLine($"\tpublic {prop.ClrType}{NullableExpression(prop)} {prop.Name} {{ get; set; }}{defaultExpr}{semicolon}");
		}

		static string NullableExpression(PropertyDefinition prop) =>
			prop.IsNullable ? "?" : string.Empty;

		static string DefaultExpression(PropertyDefinition prop)
		{
			if (!string.IsNullOrWhiteSpace(prop.DefaultValue))
				return $" = {prop.DefaultValue}";
			
			// Non-nullable string properties need default!
			if (prop.ClrType == "string" && !prop.IsNullable)
				return " = default!";
				
			return string.Empty;
		}
	}

	public static void WriteFiles(Settings settings, EntityDefinition[] entities, string outputFolder)
	{
		foreach (var entity in entities)
		{
			var baseFile = $"{entity.Name}.cs";
			var outputFile = Path.Combine(outputFolder, baseFile);

			if (File.Exists(outputFile))
			{
				Console.WriteLine($"'{baseFile}' already exists");
				continue;
			}
			
			var code = Execute(settings, entity, entities);
			File.WriteAllText(outputFile, code);
		}
	}

	private static string UsingBaseClassNamespace(Settings settings, EntityDefinition entity) =>
		string.IsNullOrWhiteSpace(settings.BaseClassNamespace) || string.IsNullOrWhiteSpace(entity.BaseClass) 
			? string.Empty 
			: $"using {settings.BaseClassNamespace};";

	private static string GetRequiredUsingStatements(Settings settings, EntityDefinition entity)
	{
		var statements = new List<string>();
		
		// Add namespace self-reference only for Generated namespace
		if (settings.DefaultNamespace == "Generated")
		{
			statements.Add("using Generated;");
		}
		
		// Add Identity using for IdentityUser base class
		if (entity.BaseClass == "IdentityUser")
		{
			statements.Add("using Microsoft.AspNetCore.Identity;");
		}
		
		return statements.Count > 0 ? string.Join("\n", statements) + "\n" : string.Empty;
	}
}
