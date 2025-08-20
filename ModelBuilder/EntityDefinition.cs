namespace ModelBuilder;

public class EntityDefinition
{
	public string Name { get; set; } = default!;
	public string? BaseClass { get; set; }
	public PropertyDefinition[] Properties { get; set; } = [];
}

public class PropertyDefinition
{
	public string Name { get; set; } = default!;
	/// <summary>
	/// clr type or referenced entity. If null, then we attempt to parse the referenced entity from the name (e.g. UserId -> User)
	/// formats allowed:
	/// - clr type (e.g. string, int, DateTime)
	/// - Entity[.NavigationProperty]
	/// </summary>
	public string? TypeExpression { get; set; } = default!;
	public int? MaxLength { get; set; }
	public bool IsNullable { get; set; }
	public string? DefaultValue { get; set; }
	public string? Comment { get; set; }
	/// <summary>
	/// is property part of unique index?
	/// </summary>
	public bool IsUnique { get; set; }
	public bool CascadeDelete { get; set; }

	/// <summary>
	/// parsed from TypeExpression, not edited directly
	/// </summary>
	public string? ClrType { get; set; }
	/// <summary>
	/// parsed from TypeExpression, not edited directly
	/// </summary>
	public string? ReferencedEntity { get; set; }	
	public string? ChildCollection { get; set; }
	public bool ParseError { get; set; }
	/// <summary>
	/// error message if parsing failed
	/// </summary>
	public string? ParseException { get; set; }
}
