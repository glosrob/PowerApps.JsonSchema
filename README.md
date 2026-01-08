# PowerApps Schema CLI

A command-line tool that extracts complete metadata schema from PowerApps/Dataverse environments to provide full context for LLMs, documentation, and development workflows.

> **Note**: This tool was built with assistance from **Claude Sonnet 4.5** (Anthropic AI). All code in this repository was generated through an AI-assisted development process.

## Quick Start (No Installation Required)

**Download and run** - No .NET installation needed:

1. Download the latest release: [powerapps-schema-win-x64.zip](https://github.com/glosrob/PowerApps.JsonSchema/releases)
2. Extract the zip file
3. Open PowerShell/Command Prompt in the extracted folder
4. Run: `.\powerapps-schema.exe extract --url https://your-org.crm.dynamics.com`

That's it! The tool will open a browser for authentication and extract your schema.

## Features

- **CLI-First Design**: Fully scriptable and automatable
- **LLM-Ready**: Perfect for providing context to AI coding assistants
- **Multiple Export Formats**: JSON, Excel (XLSX), and CSV outputs
- **Complete Metadata**: Extracts entities, attributes, relationships, option sets, and validation rules
- **Solution Filtering**: Extract schema for specific solutions or entire environments
- **Flexible Authentication**: Interactive OAuth, service principal (client ID/secret), or environment variables
- **Verbose Mode**: Detailed progress tracking when needed

## Usage

### Using Pre-built Executable (Recommended)

Download from [Releases](https://github.com/glosrob/PowerApps.JsonSchema/releases):

```bash
# Windows
.\powerapps-schema.exe extract --url https://yourorg.crm.dynamics.com
```

### Building from Source

If you prefer to build from source:

```bash
dotnet restore
dotnet build
powerapps-schema extract --url https://yourorg.crm.dynamics.com --solution MyCustomSolution
```

### Custom Output File

```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com -o custom-schema.json
```

### Export Formats

**JSON (default)** - Machine-readable format for LLMs and tooling:
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com --format json
```

**Excel (XLSX)** - Stakeholder-friendly format with one sheet per entity:
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com --format xlsx
```
Creates a workbook with:
- Summary sheet (environment, solution name, generation time, entity list)
- One sheet per entity showing:
  - Entity information (logical name, display name, custom flag, activity flag, etc.)
  - All attributes with full metadata (type, description, required level, max length, etc.)
- Option Sets sheet (all picklist values across all entities)

**CSV** - Simple flat file for basic viewing:
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com --format csv
```

### Verbose Mode

```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com -v
```

### Filtering Attributes

**Filter by prefix** - Only include attributes starting with a specific prefix (great for custom fields):
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com --attribute-prefix "new_"
```

**Exclude common fields** - Remove system fields you don't need:
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com \
  --exclude-attributes "createdon,modifiedon,createdby,modifiedby,statecode,statuscode"
```

**Combine filters**:
```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com \
  --attribute-prefix "anc_" \
  --exclude-attributes "anc_legacyid,anc_deprecated"
```

### Using Connection String (Advanced)

```bash
powerapps-schema extract -u https://yourorg.crm.dynamics.com -v
```

### Using Connection String (Advanced)

```bash
dotnet run -- extract -c "AuthType=OAuth;Username=user@org.com;Url=https://org.crm.dynamics.com;AppId=...;RedirectUri=...;LoginPrompt=Auto"
```

## CLI Options

```
powerapps-schema extract [options]

Options:
  -u, --url <url>                        Environment URL (required)
  -s, --solution <solution>              Solution unique name (optional)
  -o, --output <output>                  Output file path [default: powerapp-schema.json]
  -f, --format <format>                  Output format: json, xlsx, or csv [default: json]
  -c, --connection-string <string>       Connection string for advanced scenarios
  --client-id <clientid>                 Azure AD Application (Client) ID (or set DATAVERSE_CLIENT_ID env var)
  --client-secret <secret>               Azure AD Application Client Secret (or set DATAVERSE_CLIENT_SECRET env var)
  --attribute-prefix <prefix>            Only include attributes starting with this prefix
  --exclude-attributes <attrs>           Comma-separated list of attribute names to exclude
  -v, --verbose                          Enable verbose output
  --help                                 Show help
```

### Authentication Methods

**1. Interactive (Default)**
```bash
powerapps-schema extract --url https://org.crm.dynamics.com
```
Opens a browser for Microsoft account sign-in.

**2. Service Principal (Client Credentials)**
```bash
powerapps-schema extract --url https://org.crm.dynamics.com \
  --client-id YOUR_CLIENT_ID \
  --client-secret YOUR_CLIENT_SECRET
```

**3. Environment Variables (Best for CI/CD)**
```bash
export DATAVERSE_CLIENT_ID="your-client-id"
export DATAVERSE_CLIENT_SECRET="your-client-secret"
powerapps-schema extract --url https://org.crm.dynamics.com
```

**4. Connection String (Advanced)**
```bash
powerapps-schema extract \
  --connection-string "AuthType=ClientSecret;Url=...;ClientId=...;ClientSecret=..."
```

### Finding Your Environment URL

Your PowerApps environment URL typically looks like:
- **North America**: `https://yourorg.crm.dynamics.com`
- **Europe**: `https://yourorg.crm4.dynamics.com`
- **Asia**: `https://yourorg.crm5.dynamics.com`
- **Australia**: `https://yourorg.crm6.dynamics.com`

Find it in the Power Platform Admin Center or from your PowerApps URL.

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
powerapps-schema extract -u https://org.crm.dynamics.com -o schema.json
# Then attach schema.json to your LLM conversation
```

### CI/CD Integration
```bash
# In your pipeline
powerapps-schema extract --url $POWERAPP_URL --solution $SOLUTION_NAME --output artifacts/schema.json
```

### Automated Documentation
```bash
# Schedule daily extraction
powerapps-schema extract -u $ENV_URL -o docs/current-schema.json
```

### Schema Comparison
```bash
# Extract from different environments
powerapps-schema extract -u https://dev.crm.dynamics.com -o dev-schema.json
powerapps-schema Documentation
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
powerapps-schema extract -u https://contoso.crm.dynamics.com
```

**Solution-specific with verbose output:**
```bash
powerapps-schema extract -u https://contoso.crm.dynamics.com -s ContosoSales -v
```

**Scripted extraction:**
```powershell
# PowerShell
$solutions = @("Sales", "Marketing", "Service")
foreach ($solution in $solutions) {
    .\powerapps-schema.exe extract -u $env:ENV_URL -s $solution -o "$solution-schema.json"
}lution-specific with verbose output:**
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
