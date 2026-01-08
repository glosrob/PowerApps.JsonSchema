# Release Process

This repository uses automated releases via GitHub Actions.

## Creating a New Release

### 1. Update version (if you have a version file)
Edit any version numbers in your code if needed.

### 2. Commit your changes
```bash
git add .
git commit -m "Prepare for release v1.0.0"
git push
```

### 3. Create and push a version tag
```bash
git tag v1.0.0
git push origin v1.0.0
```

### 4. Automated process kicks in
GitHub Actions will automatically:
- Build the self-contained Windows executable
- Create a zip file
- Create a GitHub release with the tag
- Upload the zip file to the release
- Add release notes

### 5. Monitor the build
- Go to: https://github.com/glosrob/PowerApps.JsonSchema/actions
- Watch the workflow run
- Takes about 2-3 minutes

### 6. Release is live!
- Visit: https://github.com/glosrob/PowerApps.JsonSchema/releases
- Your release is published automatically

## Testing Locally First

Before creating a release, test the build locally:

```bash
# Build
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Test the executable
cd bin\Release\net8.0\win-x64\publish
.\powerapps-schema.exe extract --help
.\powerapps-schema.exe extract --url https://your-test-org.crm.dynamics.com
```

## Version Numbering

Use [Semantic Versioning](https://semver.org/):
- `v1.0.0` - Major release (breaking changes)
- `v1.1.0` - Minor release (new features, backward compatible)
- `v1.0.1` - Patch release (bug fixes)

## Example Workflow

```bash
# Make changes
git add .
git commit -m "Add new feature: custom output formatting"

# Push changes
git push

# Test locally
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# If all good, create release
git tag v1.1.0
git push origin v1.1.0

# Wait for GitHub Actions to complete (check Actions tab)
# Release is now live!
```

## Deleting a Bad Release

If something goes wrong:

```bash
# Delete tag locally
git tag -d v1.0.0

# Delete tag on GitHub
git push origin :refs/tags/v1.0.0

# Delete the release on GitHub (manually via web interface)
# Then fix issues and recreate the tag
```
