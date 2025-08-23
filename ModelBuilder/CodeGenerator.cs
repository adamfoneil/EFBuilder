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
			using Microsoft.EntityFrameworkCore;
			using Microsoft.EntityFrameworkCore.Metadata.Builders;
			{UsingBaseClassNamespace(settings)}

			namespace {settings.DefaultNamespace};

			""");

		sb.AppendLine(ClassDeclaration(entity));

		AddProperties(sb, entity);
		sb.AppendLine();
		AddParentNavProperties(sb, entity);
		AddChildCollections(sb, entity.Name, allEntities);

		sb.AppendLine("}");

		sb.AppendLine();

		sb.AppendLine($"public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>\r\n{{");
		sb.AppendLine($"\tpublic void Configure(EntityTypeBuilder<{entity.Name}> builder)\r\n\t{{");
		AddConfiguration(sb, entity);
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();

		static string ClassDeclaration(EntityDefinition e) => 
			$"public class {e.Name}" +
				(!string.IsNullOrWhiteSpace(e.BaseClass) ? $" : {e.BaseClass}" : string.Empty) + 
				"\r\n{";
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

		// Add unique indexes
		var uniqueProperties = entity.Properties.Where(p => p.IsUnique).ToArray();
		if (uniqueProperties.Any())
		{
			sb.AppendLine();
			if (uniqueProperties.Length == 1)
			{
				sb.AppendLine($"\t\tbuilder.HasIndex(e => e.{uniqueProperties[0].Name}).IsUnique().IsUnique();");
			}
			else
			{
				sb.AppendLine($"\t\tbuilder.HasIndex(e => new {{ {string.Join(", ", uniqueProperties.Select(p => $"e.{p.Name}"))} }}).IsUnique();");
			}
		}

		// Add foreign key relationships
		foreach (var prop in entity.Properties.Where(p => !string.IsNullOrWhiteSpace(p.ReferencedEntity)))
		{
			sb.AppendLine($"\t\tbuilder.HasOne(e => e.{prop.ReferencedEntity}).WithMany(e => e.{prop.ChildCollection}).HasForeignKey(x => x.{prop.Name}).OnDelete(DeleteBehavior.Restrict);");
		}
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
			sb.AppendLine($"\tpublic {prop.ClrType}{NullableExpression(prop)} {prop.Name} {{ get; set; }}{DefaultExpression(prop)};");
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

	private static string UsingBaseClassNamespace(Settings settings) =>
		string.IsNullOrWhiteSpace(settings.BaseClassNamespace) ? string.Empty : $"using {settings.BaseClassNamespace};";
}
