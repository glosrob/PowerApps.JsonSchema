# Creating a GitHub Release

## Steps to create a release:

1. **Go to GitHub Releases**:
   - Navigate to https://github.com/glosrob/PowerApps.JsonSchema/releases
   - Click "Draft a new release"

2. **Tag version**: `v1.0.0`

3. **Release title**: `PowerApps Schema CLI v1.0.0`

4. **Description**:
   ```markdown
   ## Features
   - Extract complete PowerApps/Dataverse environment metadata
   - Filter by solution
   - Interactive Microsoft authentication
   - Self-contained executable - no .NET installation required
   
   ## Download
   Download `powerapps-schema-win-x64.zip`, extract, and run.
   
   ## Usage
   ```
   powerapps-schema.exe extract --url https://your-org.crm.dynamics.com
   ```
   
   See [README](https://github.com/glosrob/PowerApps.JsonSchema#readme) for full documentation.
   ```

5. **Upload file**: Attach `releases/powerapps-schema-win-x64.zip`

6. **Publish release**

## The zip file is ready at:
`releases/powerapps-schema-win-x64.zip` (~76 MB)

This includes everything needed - users just download, extract, and run!
