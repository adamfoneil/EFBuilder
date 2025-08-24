namespace ModelBuilder;

public class EntityDefinition
{
	public string Name { get; set; } = default!;
	public string? BaseClass { get; set; }
	public PropertyDefinition[] Properties { get; set; } = [];
	public string? Comments { get; set; }
}

public class PropertyDefinition
{
	public string Name { get; set; } = default!;
	/// <summary>
	/// parsed from TypeExpression, not edited directly
	/// </summary>
	public string? ClrType { get; set; }
	/// <summary>
	/// parsed from TypeExpression, not edited directly
	/// </summary>
	public string? ReferencedEntity { get; set; }
	public int? MaxLength { get; set; }
	public bool IsNullable { get; set; }
	public string? DefaultValue { get; set; }
	public string? Comments { get; set; }
	/// <summary>
	/// is property part of unique index?
	/// </summary>
	public bool IsUnique { get; set; }
	public bool CascadeDelete { get; set; }	
	public string? ChildCollection { get; set; }
	/// <summary>
	/// indicates if this field should be auto-incrementing (ValueGeneratedOnAdd)
	/// </summary>
	public bool IsAutoIncrement { get; set; }
	public bool ParseError { get; set; }
	/// <summary>
	/// error message if parsing failed
	/// </summary>
	public string? ParseException { get; set; }
}
