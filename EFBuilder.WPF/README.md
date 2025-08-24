# EFBuilder.WPF - Entity Framework Schema Designer

A WPF application that provides a visual interface for designing Entity Framework Core schemas using the EFBuilder ModelBuilder library's markdown syntax.

## Features

### Visual Entity Designer
- **Directory-based Project Management**: Open any folder containing `.md` entity definition files
- **Entity Explorer**: Left panel displays all entities in an explorer-style list
- **Dual-View Editor**: Right panel with tabs for markdown source and generated C# output
- **Real-time Code Generation**: C# entity classes are generated automatically as you edit
- **Auto-save**: Markdown files are automatically saved when you switch entities or lose focus

### Code Generation
- **Full EF Core Support**: Generates entity classes with proper EF Core configurations
- **Relationship Handling**: Automatically resolves foreign key relationships and navigation properties
- **Constraint Management**: Supports unique indexes, required fields, field lengths, and default values
- **Base Class Inheritance**: Entities can inherit from base classes like `BaseTable`

### User Experience
- **Copy to Clipboard**: One-click copying of generated C# code
- **Syntax Highlighting**: Markdown editor uses monospace font for better readability
- **Resizable Panels**: Adjustable layout with GridSplitter
- **Status Information**: Shows currently selected entity in status bar

## Getting Started

### Prerequisites
- .NET 9.0 or later with Windows Desktop framework
- Windows operating system (WPF requirement)

### Running the Application
1. Build the solution:
   ```bash
   dotnet build EFBuilder.CLI.slnx --configuration Release
   ```

2. Run the WPF application:
   ```bash
   dotnet run --project EFBuilder.WPF --configuration Release
   ```

### Using the Application
1. **Select Directory**: Click "Browse..." to select a folder containing `.md` entity definition files
2. **Choose Entity**: Click on any entity in the left panel to load it
3. **Edit Markdown**: Use the "Markdown Source" tab to edit the entity definition
4. **View Generated Code**: Switch to "Generated C#" tab to see the resulting entity class
5. **Copy Code**: Use the "Copy to Clipboard" button to copy the generated C# code
6. **Auto-save**: Files are automatically saved when you switch entities or the markdown editor loses focus

## Entity Markdown Syntax

The application uses the EFBuilder ModelBuilder syntax for entity definitions:

```markdown
EntityName : BaseClass
#PropertyName DataType(size)
PropertyName DataType = defaultValue
ForeignKeyId
PropertyName DataType? // nullable property
```

### Example Entity Definition
```markdown
Clinic : BaseTable
#Name string(100)
IsActive bool = true
```

This generates:
```csharp
public class Clinic : BaseTable
{
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties for related entities
    public ICollection<Species> Species { get; set; } = [];
}

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Name).IsUnique();
    }
}
```

## Architecture

### Core Components
- **MainWindow**: Primary WPF window providing the user interface
- **DirectoryEntityEnumerator**: Implements `IEntityEnumerator` to read markdown files from the filesystem
- **EntityFileItem**: Data model representing an entity file in the UI

### Integration with ModelBuilder
The application integrates with the ModelBuilder library through:
- **EntityParser**: Parses markdown entity definitions into `EntityDefinition` objects
- **CodeGenerator**: Generates C# entity classes and EF Core configurations
- **IEntityEnumerator**: Interface for providing entity content from various sources

### Key Functionality
1. **File System Access**: `DirectoryEntityEnumerator` reads all `.md` files from the selected directory
2. **Real-time Parsing**: Entity definitions are parsed whenever content changes
3. **Context-aware Generation**: All entities in the directory are used for proper relationship resolution
4. **Error Handling**: Parse errors are displayed in the C# output panel

## Project Structure

```
EFBuilder.WPF/
├── App.xaml                    # WPF application definition
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main window XAML layout
├── MainWindow.xaml.cs          # Main window logic and event handlers
├── DirectoryEntityEnumerator.cs # File system entity provider
└── EFBuilder.WPF.csproj        # Project file
```

## Development and Testing

### Console Integration Test
The repository includes a console test project (`ConsoleTest`) that validates the core integration:

```bash
dotnet run --project ConsoleTest
```

This test verifies:
- DirectoryEntityEnumerator functionality
- Entity parsing from markdown files  
- C# code generation with proper EF Core configurations
- Cross-entity reference resolution

### Example Test Output
```
Found 15 entities with 1 errors
Entities:
  Breed (Base: BaseTable, Properties: 2)
  Appointment (Base: BaseTable, Properties: 14)
  [... additional entities ...]

=== Generated C# for Breed ===
using Microsoft.EntityFrameworkCore;
// [... generated entity class ...]
```

## Contributing

The WPF application is designed to be a practical tool for developers using the EFBuilder.ModelBuilder library while also serving as a test bed for discovering and creating test cases to iterate on the library itself.

Key areas for contribution:
- Enhanced UI/UX features
- Additional syntax highlighting
- Validation and error reporting improvements
- Export/import functionality
- Template management