# PowerApps Schema CLI

A command-line tool that extracts complete metadata schema from PowerApps/Dataverse environments to provide full context for LLMs, documentation, and development workflows.

## Features

- **CLI-First Design**: Fully scriptable and automatable
- **LLM-Ready**: Perfect for providing context to AI coding assistants
- **Complete Metadata**: Extracts entities, attributes, relationships, option sets, and validation rules
- **Solution Filtering**: Extract schema for specific solutions or entire environments
- **Interactive Auth**: Secure browser-based Microsoft authentication
- **Verbose Mode**: Detailed progress tracking when needed

## Installation

```bash
dotnet restore
dotnet build
```

## Usage

### Basic Command

```bash
dotnet run -- extract --url https://yourorg.crm.dynamics.com
```

### Extract Specific Solution

```bash
dotnet run -- extract --url https://yourorg.crm.dynamics.com --solution MyCustomSolution
```

### Custom Output File

```bash
dotnet run -- extract -u https://yourorg.crm.dynamics.com -o custom-schema.json
```

### Verbose Mode

```bash
dotnet run -- extract -u https://yourorg.crm.dynamics.com -v
```

### Using Connection String (Advanced)

```bash
dotnet run -- extract -c "AuthType=OAuth;Username=user@org.com;Url=https://org.crm.dynamics.com;AppId=...;RedirectUri=...;LoginPrompt=Auto"
```

## CLI Options

```
powerapps-schema extract [options]

Options:
  -u, --url <url>                    Environment URL (required)
  -s, --solution <solution>          Solution unique name (optional)
  -o, --output <output>              Output file path [default: powerapp-schema.json]
  -c, --connection-string <string>   Connection string for advanced auth
  -v, --verbose                      Enable verbose output
  --help                             Show help
```

## Output Format

The tool generates a comprehensive JSON schema:

```json
{
  "ExtractedDate": "2026-01-08T12:00:00Z",
  "EnvironmentUrl": "https://org.api.crm.dynamics.com",
  "OrganizationName": "Contoso",
  "SolutionName": "MyCustomSolution",
  "Entities": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "PrimaryIdAttribute": "accountid",
      "Attributes": [
        {
          "LogicalName": "name",
          "DisplayName": "Account Name",
          "AttributeType": "String",
          "MaxLength": 160,
          "RequiredLevel": "Required",
          "IsValidForCreate": true,
          "IsValidForUpdate": true
        }
      ]
    }
  ],
  "Relationships": [...]
}
```

## Use Cases

### LLM Context
Provide complete environment schema to AI assistants:
```bash
# Extract schema and feed to LLM
dotnet run -- extract -u https://org.crm.dynamics.com -o schema.json
# Then attach schema.json to your LLM conversation
```

### CI/CD Integration
```bash
# In your pipeline
dotnet run -- extract --url $POWERAPP_URL --solution $SOLUTION_NAME --output artifacts/schema.json
```

### Automated Documentation
```bash
# Schedule daily extraction
dotnet run -- extract -u $ENV_URL -o docs/current-schema.json
```

### Schema Comparison
```bash
# Extract from different environments
dotnet run -- extract -u https://dev.crm.dynamics.com -o dev-schema.json
dotnet run -- extract -u https://prod.crm.dynamics.com -o prod-schema.json
# Then compare files
```

## Authentication

The tool uses interactive browser-based Microsoft authentication by default:
1. A browser window opens
2. Sign in with your Microsoft account
3. Grant consent if prompted
4. The tool caches credentials for future use

**Required Permissions**: System Administrator or System Customizer role

## Building for Distribution

```bash
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Executable will be in: bin/Release/net8.0/win-x64/publish/powerapps-schema.exe
```

Then you can run directly:
```bash
powerapps-schema extract -u https://org.crm.dynamics.com
```

## Examples

**Quick extraction:**
```bash
dotnet run -- extract -u https://contoso.crm.dynamics.com
```

**Solution-specific with verbose output:**
```bash
dotnet run -- extract -u https://contoso.crm.dynamics.com -s ContosoSales -v
```

**Scripted extraction:**
```bash
#!/bin/bash
for solution in Sales Marketing Service; do
  dotnet run -- extract -u $ENV_URL -s $solution -o ${solution}-schema.json
done
```

## Troubleshooting

**"Failed to connect"**: Verify URL format and network connectivity

**"Solution not found"**: Use the unique name (not display name), case-sensitive

**Authentication loops**: Clear cached credentials: `rm -rf ~/.IdentityService`

## License

MIT
