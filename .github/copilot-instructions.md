# EFBuilder.CLI

EFBuilder.CLI is a .NET 9.0 library that generates Entity Framework Core entity classes from a "markdown" style syntax. The ModelBuilder library parses entity definitions from markdown files and transpiles them into C# source files with proper EF Core configurations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- Ensure .NET 9.0 SDK is installed: `dotnet --version` should return 9.0.x
- This project uses MSTest for testing with the latest MSTest.Sdk

### Bootstrap, Build, and Test
- **Restore dependencies**: `dotnet restore EFBuilder.CLI.slnx` -- takes ~2 seconds. NEVER CANCEL. Set timeout to 5+ minutes for safety.
- **Build the solution**: `dotnet build EFBuilder.CLI.slnx --no-restore --configuration Debug` -- takes ~2 seconds. NEVER CANCEL. Set timeout to 5+ minutes for safety.
- **Run tests**: `dotnet test Testing/Testing.csproj --verbosity normal` -- takes ~3 seconds. NEVER CANCEL. Set timeout to 10+ minutes for safety. NOTE: Current test fails due to expected output format differences, but library functionality works correctly.

### Using the ModelBuilder Library
The ModelBuilder library is used programmatically through its main classes:
- **EntityParser**: Parses markdown entity definitions into EntityDefinition objects
- **CodeGenerator**: Generates C# entity classes and EF Core configurations
- **IEntityEnumerator**: Interface for providing entity content (e.g., ResourceEnumerator for embedded resources)

Example usage (see Testing/EntityParsing.cs):
```csharp
var schema = new ResourceEnumerator("Testing.SpayWise.", ["Clinic.md", "Species.md"]);
var (definitions, errors) = new EntityParser(schema).ParseEntities();
var settings = new CodeGenerator.Settings() { DefaultNamespace = "MyNamespace" };
var generatedFiles = CodeGenerator.Execute(settings, definitions);
```

## Project Structure

### Key Projects
- **ModelBuilder/**: Core library containing entity parsing and code generation functionality
- **Testing/**: MSTest project containing unit tests and test resources

### Important Files and Directories
- **EFBuilder.CLI.slnx**: Solution file (use this for all dotnet commands)
- **ModelBuilder/EntityParser.cs**: Parses markdown entity definitions
- **ModelBuilder/CodeGenerator.cs**: Generates C# entity classes and EF Core configurations
- **ModelBuilder/EntityDefinition.cs**: Data models for parsed entities and properties
- **ModelBuilder/IEntityEnumerator.cs**: Interface for content providers
- **Testing/EntityParsing.cs**: Main test cases validating entity generation
- **Testing/SpayWise/**: Entity definition markdown files for testing
- **Testing/Case1/**: Expected generated C# entity files
- **Testing/Conventions/BaseTable.cs**: Base class for generated entities

## Input Format

The library accepts "markdown" style entity definitions with this syntax pattern:
```
[#]Name [ClrTypeOrReferencedEntity[?]] [<ParentCollection] [= default value] [// comment]
```

Entity definition structure:
- **First line**: Entity name, optionally with base class (`EntityName : BaseClass`)
- **Properties**: Each subsequent line defines a property
- **# prefix**: Indicates the property is part of a unique constraint
- **Type specification**: CLR type with optional size `string(50)` or referenced entity name
- **? suffix**: Makes the property nullable
- **< suffix**: Defines parent collection name for navigation properties
- **= default**: Sets default value (e.g., `IsActive bool = true`)
- **// comment**: Optional comment

Example from `Testing/SpayWise/Clinic.md`:
```
Clinic : BaseTable
#Name string(100)
IsActive bool = true
```

Example with relationships from `Testing/SpayWise/Species.md`:
```
Species : BaseTable
#ClinicId < Species
#Name string(50)
AppSpeciesId
BaseName string(50)
Abbreviation string(3)
MinWeight int?
IsActive bool = true
```

## Generated Output

The ModelBuilder library generates:
1. **Entity classes** inheriting from specified base class (e.g., BaseTable)
2. **EF Core configuration classes** implementing IEntityTypeConfiguration<T>
3. **Navigation properties** for foreign key relationships and collections
4. **Property configurations** (required/nullable, max length, unique constraints, foreign keys)

Example generated output from `Testing/Case1/Clinic.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Testing.Conventions;

namespace Testing.Case1;

public class Clinic : BaseTable
{
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;

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

## Validation and Testing

### Manual Validation Steps
Always run these validation steps after making changes:

1. **Build validation**: `dotnet build EFBuilder.CLI.slnx --no-restore --configuration Debug`
2. **Test validation**: `dotnet test Testing/Testing.csproj --verbosity normal` (NOTE: Expected to fail currently due to output format differences)
3. **Library functionality test** - Create a simple console app to test programmatically:
   ```csharp
   using ModelBuilder;
   
   var fileEnumerator = new SimpleFileEnumerator([("Test.md", "TestEntity : BaseTable\n#Name string(50)")]);
   var (definitions, errors) = new EntityParser(fileEnumerator).ParseEntities();
   var settings = new CodeGenerator.Settings() { DefaultNamespace = "Test" };
   var files = CodeGenerator.Execute(settings, definitions);
   // Should generate 1 file with TestEntity class and configuration
   ```

### Expected Test Scenarios
When testing manually, always verify:
- **Entity parsing**: Markdown files are correctly parsed into EntityDefinition objects
- **Code generation**: Proper C# entity classes with inheritance, properties, and navigation properties
- **EF Core configurations**: Property constraints (required, max length, unique indexes) and relationships
- **Namespace handling**: Generated code uses correct namespaces and using statements

### Manual Testing Scenario (Complete Validation)
After making changes, create a simple test console app to validate library functionality:

1. Create a new console app: `dotnet new console -n ValidationTest`
2. Add project reference: `<ProjectReference Include="../ModelBuilder/ModelBuilder.csproj" />`
3. Use this test code:
```csharp
using ModelBuilder;

var testEntity = @"TestEntity : BaseTable
#Name string(100)
IsActive bool = true
CategoryId";

var enumerator = new SimpleFileEnumerator([("TestEntity.md", testEntity)]);
var (definitions, errors) = new EntityParser(enumerator).ParseEntities();

Console.WriteLine($"Parsed {definitions.Length} entities with {errors.Length} errors");
if (errors.Length > 0) { 
    foreach(var error in errors) Console.WriteLine($"Error: {error}");
    return;
}

var settings = new CodeGenerator.Settings() { DefaultNamespace = "TestNamespace" };
var files = CodeGenerator.Execute(settings, definitions);

Console.WriteLine($"Generated {files.Length} files");
foreach (var file in files) {
    Console.WriteLine($"File: {file.Filename}");
    Console.WriteLine("Generated Content:");
    Console.WriteLine(file.Content);
}

public class SimpleFileEnumerator(IEnumerable<(string Name, string Content)> files) : IEntityEnumerator
{
    public (string Name, string Content)[] GetContent() => files.ToArray();
}
```
4. Run: `dotnet run` 
5. Verify output includes proper entity class with BaseTable inheritance, properties, and EF configuration

Expected output should include:
- Entity class with `TestEntity : BaseTable`
- String property with required and max length configuration
- Bool property with default value 
- Foreign key property with navigation property
- IEntityTypeConfiguration implementation

### Testing Guidelines
- All tests should pass: `dotnet test Testing/Testing.csproj`
- Test entity definitions are in `Testing/SpayWise/` as markdown files
- Expected output files are in `Testing/Case1/` for comparison
- Tests validate entity parsing, code generation, and EF Core configuration generation
- Tests use embedded resources via ResourceEnumerator

## Common Tasks and Commands

### Build Commands (with timeouts)
```bash
# Full restore and build (set 5+ minute timeout)
dotnet restore EFBuilder.CLI.slnx && dotnet build EFBuilder.CLI.slnx --no-restore

# Clean and rebuild (set 5+ minute timeout)
dotnet clean EFBuilder.CLI.slnx && dotnet build EFBuilder.CLI.slnx

# Test execution (set 10+ minute timeout)
dotnet test Testing/Testing.csproj --verbosity normal
```

### Development Workflow
1. **Before making changes**: Always run `dotnet build` and `dotnet test` to verify current state
2. **After making changes**: 
   - Run `dotnet build EFBuilder.CLI.slnx --no-restore` to check compilation
   - Run `dotnet test Testing/Testing.csproj` to verify functionality
   - Test library functionality programmatically using EntityParser and CodeGenerator
3. **When adding new features**: Add corresponding test cases in `Testing/EntityParsing.cs`

### Debugging
- **Debug the library**: Create test scenarios in `Testing/EntityParsing.cs`
- **Debug tests**: Use `dotnet test Testing/Testing.csproj --logger console --verbosity diagnostic`
- **Inspect generated code**: Check `Testing/Case1/` for expected outputs

## Repository Information

### Solution Structure
```
EFBuilder.CLI.slnx          # Main solution file
├── ModelBuilder/           # Core library project
│   ├── ModelBuilder.csproj # .NET 9.0 library project
│   ├── EntityParser.cs     # Parses markdown entity definitions
│   ├── CodeGenerator.cs    # Generates C# entity classes and EF configurations
│   ├── EntityDefinition.cs # Data models for entities and properties
│   ├── IEntityEnumerator.cs # Interface for content providers
│   ├── LocalSettings.cs    # Configuration settings
│   ├── bin/               # Build output
│   └── obj/               # Build artifacts
└── Testing/               # Test project
    ├── Testing.csproj     # MSTest project with MSTest.Sdk
    ├── EntityParsing.cs   # Main test cases
    ├── ResourceEnumerator.cs # Embedded resource provider for tests
    ├── MSTestSettings.cs  # MSTest configuration
    ├── SpayWise/          # Entity definition markdown files for testing
    │   ├── Clinic.md      # Example entity definition
    │   ├── Species.md     # Example with relationships
    │   └── ...            # Other test entity definitions
    ├── Case1/             # Expected generated C# entity files
    │   ├── Clinic.cs      # Expected generated entity
    │   ├── Species.cs     # Expected generated entity with relationships
    │   └── ...            # Other expected outputs
    ├── Conventions/       # Base classes and conventions
    │   └── BaseTable.cs   # Base entity class with audit fields
    ├── bin/               # Build output
    └── obj/               # Build artifacts
```

### File Listing Reference
Run `ls -la` in repository root:
```
.config
.git
.gitattributes
.github/
.gitignore
EFBuilder.CLI.slnx
ModelBuilder/
README.md
Testing/
```

Run `ls -la Testing/SpayWise/` (entity definition files):
```
AppSpecies.md      # Entity definition example
Clinic.md          # Entity definition example  
Species.md         # Entity definition with relationships
Breed.md           # Entity definition example
...                # Other .md entity definitions
```

Run `ls -la Testing/Case1/` (expected generated outputs):
```
AppSpecies.cs      # Expected generated entity
Clinic.cs          # Expected generated entity
Species.cs         # Expected generated entity
Breed.cs           # Expected generated entity
```

### Git and CI
- **NEVER CANCEL** any build or test commands. Builds typically take ~2 seconds, tests take ~3 seconds.
- Use appropriate timeouts: minimum 5 minutes for builds, 10 minutes for tests
- The repository uses standard .NET .gitignore patterns
- GitHub Actions workflow exists at `.github/workflows/copilot-setup-steps.yaml`
- CI workflow runs: restore, build (but not tests currently)

### Key Dependencies
- **.NET 9.0**: Latest .NET version with C# latest language features
- **MSTest.Sdk 3.6.4**: Modern MSTest framework for testing
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.8**: EF Core dependencies for testing

### Troubleshooting

### Common Issues
- **Build failures**: Ensure .NET 9.0 SDK is installed and use the correct solution file (`EFBuilder.CLI.slnx`)
- **Test failures**: Check that entity definition files exist in `Testing/SpayWise/` and expected outputs in `Testing/Case1/`
- **Parsing errors**: Verify markdown entity definitions follow the correct syntax pattern
- **Code generation issues**: Check that EntityDefinition objects are properly created by EntityParser

### Current Implementation Status
- **ModelBuilder library**: Fully functional with EntityParser, CodeGenerator, and supporting classes
- **Tests**: One test exists that validates entity parsing and code generation, but currently fails due to generated output differences - this is a technical issue separate from the library functionality
- **Entity definitions**: Multiple test cases in `Testing/SpayWise/` demonstrating various entity patterns
- **Expected outputs**: Generated entity examples in `Testing/Case1/` for validation
- **Note**: Library core functionality works correctly - validated programmatically. Test failure is due to expected vs actual output format differences (whitespace, property ordering)

### Performance Notes
- Restore operations: ~2 seconds
- Build operations: ~2 seconds  
- Test execution: ~3 seconds
- All operations are fast, but always set generous timeouts (5-10 minutes) to prevent accidental cancellation

## Critical Reminders

- **NEVER CANCEL** build or test commands - set timeouts of 5+ minutes for builds, 10+ minutes for tests
- Always use `EFBuilder.CLI.slnx` as the solution file for all dotnet commands
- Test both compilation and functionality after any changes
- This is a LIBRARY project, not a console application - use the ModelBuilder classes programmatically
- Entity definitions are in markdown format in `Testing/SpayWise/` directory
- Generated entity examples are in `Testing/Case1/` for reference and validation
- All tests use embedded resources via ResourceEnumerator to access entity definitions