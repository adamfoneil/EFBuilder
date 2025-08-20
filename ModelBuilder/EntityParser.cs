namespace ModelBuilder;

public class EntityParser(IEntityEnumerator contentAccessor)
{
	private readonly IEntityEnumerator _contentAccessor = contentAccessor;

	private static string[] _clrTypes => 
	[	
		"int", "long", "string", "DateTime", "bool", "decimal",
		"float", "double", "Guid", "byte[]", "char", "DateOnly", "TimeOnly"
	];

	public (EntityDefinition[] EntityDefinitions, string[] Errors) ParseEntities()
	{
		var entitySources = _contentAccessor.GetEntitySources();
		var entities = new List<EntityDefinition>();
		var entityNames = entities.Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

		List<string> errors = [];

		foreach (var (name, source) in entitySources)
		{
			try
			{				
				entities.Add(ParseEntity(source, entityNames));				
			}
			catch (Exception ex)
			{
				errors.Add($"{name}: {ex.Message}");
			}
		}

		return ([.. entities], [..errors]);
	}

	private EntityDefinition ParseEntity(string source, HashSet<string> entityNames)
	{
		throw new NotImplementedException();
	}
}
