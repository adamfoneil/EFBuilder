using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace ModelBuilder;

/// <summary>
/// SQL Server implementation of database scaffolding
/// </summary>
public class SqlServerScaffolder : IScaffolder
{
    private readonly Dictionary<string, string> _typeMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        // Text types
        ["varchar"] = "string",
        ["nvarchar"] = "string", 
        ["char"] = "string",
        ["nchar"] = "string",
        ["text"] = "string",
        ["ntext"] = "string",
        
        // Numeric types
        ["int"] = "int",
        ["bigint"] = "long",
        ["smallint"] = "short",
        ["tinyint"] = "byte",
        ["decimal"] = "decimal",
        ["numeric"] = "decimal",
        ["money"] = "decimal",
        ["smallmoney"] = "decimal",
        ["float"] = "double",
        ["real"] = "float",
        ["bit"] = "bool",
        
        // Date/time types
        ["datetime"] = "DateTime",
        ["datetime2"] = "DateTime",
        ["smalldatetime"] = "DateTime",
        ["date"] = "DateOnly",
        ["time"] = "TimeOnly",
        ["datetimeoffset"] = "DateTimeOffset",
        
        // Other types
        ["uniqueidentifier"] = "Guid",
        ["binary"] = "byte[]",
        ["varbinary"] = "byte[]",
        ["image"] = "byte[]",
        ["timestamp"] = "byte[]",
        ["rowversion"] = "byte[]"
    };

    public Dictionary<string, string> GetTypeMapping() => new(_typeMapping, StringComparer.OrdinalIgnoreCase);

    public async Task<(string FileName, string Content)[]> ScaffoldEntitiesAsync(string connectionString)
    {
        var entities = new List<(string FileName, string Content)>();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Get all tables
        var tables = await GetTablesAsync(connection);
        
        foreach (var table in tables)
        {
            var content = await GenerateEntityMarkdownAsync(connection, table);
            entities.Add(($"{table}.md", content));
        }
        
        return entities.ToArray();
    }

    private async Task<string[]> GetTablesAsync(SqlConnection connection)
    {
        var tables = new List<string>();
        
        var query = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE'
            AND TABLE_SCHEMA = 'dbo'
            ORDER BY TABLE_NAME";
            
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString("TABLE_NAME"));
        }
        
        return tables.ToArray();
    }

    private async Task<string> GenerateEntityMarkdownAsync(SqlConnection connection, string tableName)
    {
        var sb = new StringBuilder();
        
        // Entity header - assume BaseTable as base class for now
        sb.AppendLine($"{tableName} : BaseTable");
        
        // Get columns
        var columns = await GetColumnsAsync(connection, tableName);
        var foreignKeys = await GetForeignKeysAsync(connection, tableName);
        var primaryKeys = await GetPrimaryKeysAsync(connection, tableName);
        var uniqueConstraints = await GetUniqueConstraintsAsync(connection, tableName);
        
        foreach (var column in columns)
        {
            var line = GeneratePropertyLine(column, foreignKeys, primaryKeys, uniqueConstraints);
            if (!string.IsNullOrEmpty(line))
            {
                sb.AppendLine(line);
            }
        }
        
        return sb.ToString();
    }

    private string GeneratePropertyLine(ColumnInfo column, 
        ForeignKeyInfo[] foreignKeys, 
        string[] primaryKeys, 
        UniqueConstraintInfo[] uniqueConstraints)
    {
        var sb = new StringBuilder();
        
        // Check if column is part of unique constraint (but not primary key)
        var isUnique = uniqueConstraints.Any(uc => uc.ColumnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase)) &&
                      !primaryKeys.Contains(column.Name, StringComparer.OrdinalIgnoreCase);
        
        if (isUnique)
        {
            sb.Append('#');
        }
        
        sb.Append(column.Name);
        
        // Check if this is a foreign key
        var fk = foreignKeys.FirstOrDefault(fk => fk.ColumnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase));
        
        if (fk != null)
        {
            // Reference another entity
            sb.Append($" {fk.ReferencedTable}");
            if (column.IsNullable)
            {
                sb.Append('?');
            }
            
            // Add collection hint
            var collectionName = GetPluralName(column.Name.EndsWith("Id") ? column.Name[..^2] : column.Name);
            sb.Append($" <{collectionName}");
        }
        else
        {
            // Regular property
            var clrType = GetClrType(column);
            if (!string.IsNullOrEmpty(clrType))
            {
                sb.Append($" {clrType}");
                
                if (column.IsNullable && clrType != "string")
                {
                    sb.Append('?');
                }
                
                // Add max length for string types
                if (clrType == "string" && column.MaxLength > 0 && column.MaxLength < int.MaxValue)
                {
                    sb = new StringBuilder(sb.ToString().Replace(" string", $" string({column.MaxLength})"));
                }
            }
        }
        
        // Add default value if present
        if (!string.IsNullOrEmpty(column.DefaultValue))
        {
            var defaultValue = ParseDefaultValue(column.DefaultValue, column.DataType);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                sb.Append($" = {defaultValue}");
            }
        }
        
        return sb.ToString();
    }

    private string GetClrType(ColumnInfo column)
    {
        return _typeMapping.TryGetValue(column.DataType, out var clrType) ? clrType : "object";
    }

    private string ParseDefaultValue(string defaultValue, string dataType)
    {
        if (string.IsNullOrWhiteSpace(defaultValue))
            return string.Empty;
            
        // Remove parentheses and quotes
        defaultValue = defaultValue.Trim('(', ')', '\'', '"');
        
        // Handle common SQL Server default patterns
        if (defaultValue.Equals("getdate()", StringComparison.OrdinalIgnoreCase) ||
            defaultValue.Equals("getutcdate()", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty; // Don't include date functions as defaults
        }
        
        if (dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
        {
            return defaultValue == "1" ? "true" : "false";
        }
        
        if (IsNumericType(dataType))
        {
            return defaultValue;
        }
        
        if (IsStringType(dataType))
        {
            return $"\"{defaultValue}\"";
        }
        
        return string.Empty;
    }

    private bool IsNumericType(string dataType) =>
        new[] { "int", "bigint", "smallint", "tinyint", "decimal", "numeric", "money", "smallmoney", "float", "real" }
            .Contains(dataType, StringComparer.OrdinalIgnoreCase);

    private bool IsStringType(string dataType) =>
        new[] { "varchar", "nvarchar", "char", "nchar", "text", "ntext" }
            .Contains(dataType, StringComparer.OrdinalIgnoreCase);

    private string GetPluralName(string name)
    {
        // Simple pluralization - could be enhanced
        if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "ies";
        if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            return name + "es";
        return name + "s";
    }

    private async Task<ColumnInfo[]> GetColumnsAsync(SqlConnection connection, string tableName)
    {
        var columns = new List<ColumnInfo>();
        
        var query = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_NAME = @tableName
            AND c.TABLE_SCHEMA = 'dbo'
            ORDER BY c.ORDINAL_POSITION";
            
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString("COLUMN_NAME"),
                DataType = reader.GetString("DATA_TYPE"),
                IsNullable = reader.GetString("IS_NULLABLE").Equals("YES", StringComparison.OrdinalIgnoreCase),
                MaxLength = reader.IsDBNull("CHARACTER_MAXIMUM_LENGTH") ? 0 : reader.GetInt32("CHARACTER_MAXIMUM_LENGTH"),
                DefaultValue = reader.IsDBNull("COLUMN_DEFAULT") ? null : reader.GetString("COLUMN_DEFAULT")
            });
        }
        
        return columns.ToArray();
    }

    private async Task<ForeignKeyInfo[]> GetForeignKeysAsync(SqlConnection connection, string tableName)
    {
        var foreignKeys = new List<ForeignKeyInfo>();
        
        var query = @"
            SELECT 
                fk_kcu.COLUMN_NAME,
                pk_kcu.TABLE_NAME AS REFERENCED_TABLE_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk_kcu
            INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                ON fk_kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk_kcu
                ON rc.UNIQUE_CONSTRAINT_NAME = pk_kcu.CONSTRAINT_NAME
            WHERE fk_kcu.TABLE_NAME = @tableName
            AND fk_kcu.TABLE_SCHEMA = 'dbo'";
            
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            foreignKeys.Add(new ForeignKeyInfo
            {
                ColumnName = reader.GetString("COLUMN_NAME"),
                ReferencedTable = reader.GetString("REFERENCED_TABLE_NAME")
            });
        }
        
        return foreignKeys.ToArray();
    }

    private async Task<string[]> GetPrimaryKeysAsync(SqlConnection connection, string tableName)
    {
        var primaryKeys = new List<string>();
        
        var query = @"
            SELECT kcu.COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
            WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            AND kcu.TABLE_NAME = @tableName
            AND kcu.TABLE_SCHEMA = 'dbo'";
            
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            primaryKeys.Add(reader.GetString("COLUMN_NAME"));
        }
        
        return primaryKeys.ToArray();
    }

    private async Task<UniqueConstraintInfo[]> GetUniqueConstraintsAsync(SqlConnection connection, string tableName)
    {
        var uniqueConstraints = new List<UniqueConstraintInfo>();
        
        var query = @"
            SELECT kcu.COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
            WHERE tc.CONSTRAINT_TYPE = 'UNIQUE'
            AND kcu.TABLE_NAME = @tableName
            AND kcu.TABLE_SCHEMA = 'dbo'";
            
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            uniqueConstraints.Add(new UniqueConstraintInfo
            {
                ColumnName = reader.GetString("COLUMN_NAME")
            });
        }
        
        return uniqueConstraints.ToArray();
    }

    private class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public int MaxLength { get; set; }
        public string? DefaultValue { get; set; }
    }

    private class ForeignKeyInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public string ReferencedTable { get; set; } = string.Empty;
    }

    private class UniqueConstraintInfo
    {
        public string ColumnName { get; set; } = string.Empty;
    }
}