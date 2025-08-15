# EFBuilder.CLI

EFBuilder.CLI is a .NET 9.0 console application that generates Entity Framework Core entity classes from a "markdown" style syntax. The tool parses input files containing entity definitions and transpiles them into C# source files with proper EF Core configurations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- Ensure .NET 9.0 SDK is installed: `dotnet --version` should return 9.0.x
- This project uses MSTest for testing with the latest MSTest.Sdk

### Bootstrap, Build, and Test
- **Restore dependencies**: `dotnet restore EFBuilder.CLI.slnx` -- takes 2-3 seconds. NEVER CANCEL.
- **Build the solution**: `dotnet build EFBuilder.CLI.slnx --no-restore --configuration Debug` -- takes 2-3 seconds. NEVER CANCEL. Set timeout to 5+ minutes for safety.
- **Run tests**: `dotnet test Testing/Testing.csproj --verbosity normal` -- takes 5-6 seconds. NEVER CANCEL. Set timeout to 10+ minutes for safety.

### Running the CLI Application
- **Run without arguments** (shows usage): `cd EFBuilder && dotnet run`
- **Run with input file**: `cd EFBuilder && dotnet run <input-file> [output-directory]`
- **Example**: `cd EFBuilder && dotnet run ../Testing/Resources/Case01/input.txt ../tmp/output`

**IMPORTANT**: The current Program.cs only contains a template ("Hello, World!"). When implementing the CLI, refer to the repository context and test cases to understand the expected functionality. The tests in Testing/Test1.cs show how EFBuilderService should work.

## Project Structure

### Key Projects
- **EFBuilder/**: Main console application project containing the CLI entry point
- **Testing/**: MSTest project containing unit tests and test resources

### Important Files and Directories
- **EFBuilder.CLI.slnx**: Solution file (use this for all dotnet commands)
- **EFBuilder/Program.cs**: CLI entry point and argument parsing
- **EFBuilder/EFBuilderService.cs**: Main service orchestrating entity generation
- **EFBuilder/Services/**: Core services (EntityParser, CodeGenerator)
- **EFBuilder/Models/**: Data models (EntityDefinition, PropertyDefinition)
- **Testing/Test1.cs**: Main test cases validating entity generation
- **Testing/Resources/Case01/**: Test input files and expected outputs
- **Testing/Conventions/BaseTable.cs**: Base class for generated entities

## Input Format

The tool accepts "markdown" style entity definitions:

```
#EntityName : BaseClass
PropertyName type(length)
OptionalProperty type?
PropertyWithDefault type = defaultValue

#AnotherEntity : BaseTable
ForeignKeyId
NavigationProperty
```

Example from `Testing/Resources/Case01/input.txt`:
```
#Customer : BaseTable
FirstName string(50)
LastName string(50)
Email string(50)?
Address string(50)?
Balance decimal
IsActive bool = true

#Status : BaseTable
Name string(50)
Description string(255)?

#Order : BaseTable
CustomerId
Date datetime
StatusId
StatusDate datetime?
TotalAmount decimal
```

## Generated Output

The tool generates:
1. **Entity classes** inheriting from specified base class (e.g., BaseTable)
2. **EF Core configuration classes** implementing IEntityTypeConfiguration<T>
3. **Navigation properties** for foreign key relationships
4. **Property configurations** (required/nullable, max length, foreign keys)

## Validation and Testing

### Manual Validation Steps
Always run these validation steps after making changes:

1. **Build validation**: `dotnet build EFBuilder.CLI.slnx --no-restore --configuration Debug`
2. **Test validation**: `dotnet test Testing/Testing.csproj --verbosity normal`
3. **CLI functionality test**:
   ```bash
   cd EFBuilder
   dotnet run ../Testing/Resources/Case01/input.txt ../tmp/output
   # Verify generated files exist and contain expected content
   ls -la ../tmp/output/
   cat ../tmp/output/Customer.cs
   ```

### Expected Test Scenarios
When testing manually, always verify:
- **Customer.cs generation**: Contains proper inheritance, required properties, navigation properties
- **Status.cs generation**: Contains proper property configurations and relationships
- **Order.cs generation**: Contains foreign key properties and navigation properties
- **EF Core configurations**: Property constraints (required, max length) and foreign key relationships

### Testing Guidelines
- All tests should pass: `dotnet test Testing/Testing.csproj`
- Test files are located in `Testing/Resources/Case01/`
- Expected output files are already generated in the same directory for comparison
- Tests validate both entity class generation and EF Core configuration generation

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
   - Test CLI manually with sample input files
3. **When adding new features**: Add corresponding test cases in `Testing/Test1.cs`

### Debugging
- **Debug the console app**: Use `cd EFBuilder && dotnet run --` with debugging arguments
- **Debug tests**: Use `dotnet test Testing/Testing.csproj --logger console --verbosity diagnostic`

## Repository Information

### Solution Structure
```
EFBuilder.CLI.slnx          # Main solution file
├── EFBuilder/              # Console application
│   ├── EFBuilder.csproj    # .NET 9.0 executable project
│   ├── Program.cs          # CLI entry point (currently template)
│   ├── bin/                # Build output
│   └── obj/                # Build artifacts
└── Testing/                # Test project
    ├── Testing.csproj      # MSTest project with MSTest.Sdk
    ├── Test1.cs            # Main test cases (currently template)
    ├── MSTestSettings.cs   # MSTest configuration
    ├── Resources/          # Test input and expected output files
    │   └── Case01/         # Example test case
    │       ├── input.txt   # Markdown-style entity definitions
    │       ├── Customer.cs # Expected generated Customer entity
    │       ├── Order.cs    # Expected generated Order entity
    │       └── Status.cs   # Expected generated Status entity
    ├── Conventions/        # Base classes and conventions
    │   └── BaseTable.cs    # Base entity class with audit fields
    ├── bin/                # Build output
    └── obj/                # Build artifacts
```

### File Listing Reference
Run `ls -la` in repository root:
```
.git
.gitattributes
.github/
.gitignore
EFBuilder/
EFBuilder.CLI.slnx
README.md
Testing/
```

Run `ls -la Testing/Resources/Case01/`:
```
Customer.cs    # Generated entity example
Order.cs       # Generated entity example  
Status.cs      # Generated entity example
input.txt      # Input file example
```

### Git and CI
- **NEVER CANCEL** any build or test commands. Builds typically take 2-3 seconds, tests take 5-6 seconds.
- Use appropriate timeouts: minimum 5 minutes for builds, 10 minutes for tests
- The repository uses standard .NET .gitignore patterns
- GitHub Actions workflow exists at `.github/workflows/copilot-setup-steps.yaml`

### Key Dependencies
- **.NET 9.0**: Latest .NET version with C# latest language features
- **MSTest.Sdk 3.6.4**: Modern MSTest framework
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.8**: EF Core dependencies for testing

### Troubleshooting

### Common Issues
- **Build failures**: Ensure .NET 9.0 SDK is installed and use the correct solution file (`EFBuilder.CLI.slnx`)
- **Test failures**: Check that test resource files exist in `Testing/Resources/Case01/`
- **CLI not working**: The current Program.cs only contains a template. The actual CLI logic needs to be implemented based on the repository context and test cases.

### Current Implementation Status
- **Tests**: Pass but only contain placeholder test (needs actual entity generation tests)
- **CLI**: Only shows "Hello, World!" (needs proper argument parsing and EFBuilderService integration)
- **Services**: Not yet implemented (need EFBuilderService, EntityParser, CodeGenerator classes)
- **Test Resources**: Available in `Testing/Resources/Case01/` showing expected input/output format

### Performance Notes
- Restore operations: ~2-3 seconds
- Build operations: ~2-3 seconds  
- Test execution: ~4-5 seconds
- All operations are fast, but always set generous timeouts (5-10 minutes) to prevent accidental cancellation

## Critical Reminders

- **NEVER CANCEL** build or test commands - set timeouts of 5+ minutes for builds, 10+ minutes for tests
- Always use `EFBuilder.CLI.slnx` as the solution file for all dotnet commands
- Test both compilation and functionality after any changes
- When implementing the CLI logic, ensure it properly parses command line arguments and calls EFBuilderService
- Generated entity classes should inherit from BaseTable and include proper EF Core configurations