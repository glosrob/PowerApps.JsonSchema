using Microsoft.PowerPlatform.Dataverse.Client;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerApps.JsonSchema;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("PowerApps Schema Extractor - Extract metadata schema from PowerApps/Dataverse environments");

        var extractCommand = new Command("extract", "Extract schema from a PowerApps environment");

        var urlOption = new Option<string>(
            aliases: new[] { "--url", "-u" },
            description: "PowerApps environment URL (e.g., https://org.crm.dynamics.com)")
        {
            IsRequired = true
        };

        var solutionOption = new Option<string?>(
            aliases: new[] { "--solution", "-s" },
            description: "Solution unique name to filter by (optional - extracts all metadata if not specified)");

        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            getDefaultValue: () => "powerapp-schema.json",
            description: "Output file path");

        var connectionStringOption = new Option<string?>(
            aliases: new[] { "--connection-string", "-c" },
            description: "Dataverse connection string (alternative to individual auth options)");

        var clientIdOption = new Option<string?>(
            aliases: new[] { "--client-id" },
            description: "Azure AD Application (Client) ID for service principal authentication (or set DATAVERSE_CLIENT_ID env var)");

        var clientSecretOption = new Option<string?>(
            aliases: new[] { "--client-secret" },
            description: "Azure AD Application Client Secret for service principal authentication (or set DATAVERSE_CLIENT_SECRET env var)");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");

        extractCommand.AddOption(urlOption);
        extractCommand.AddOption(solutionOption);
        extractCommand.AddOption(outputOption);
        extractCommand.AddOption(connectionStringOption);
        extractCommand.AddOption(clientIdOption);
        extractCommand.AddOption(clientSecretOption);
        extractCommand.AddOption(verboseOption);

        extractCommand.SetHandler(async (url, solution, output, connectionString, clientId, clientSecret, verbose) =>
        {
            await ExtractSchemaAsync(url, solution, output, connectionString, clientId, clientSecret, verbose);
        }, urlOption, solutionOption, outputOption, connectionStringOption, clientIdOption, clientSecretOption, verboseOption);

        rootCommand.AddCommand(extractCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ExtractSchemaAsync(string url, string? solution, string output, string? connectionString, string? clientId, string? clientSecret, bool verbose)
    {
        try
        {
            if (verbose)
            {
                Console.WriteLine("PowerApps Schema Extractor");
                Console.WriteLine("===========================\n");
                Console.WriteLine($"Environment URL: {url}");
                Console.WriteLine($"Solution: {solution ?? "(all metadata)"}");
                Console.WriteLine($"Output: {output}\n");
            }

            Console.WriteLine("Connecting to PowerApps environment...");

            // Check for environment variables if options not provided
            clientId ??= Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_ID");
            clientSecret ??= Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_SECRET");

            ServiceClient serviceClient;
            
            try
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    // Use provided connection string
                    if (verbose)
                    {
                        Console.WriteLine("Using provided connection string");
                    }
                    serviceClient = new ServiceClient(connectionString);
                }
                else if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                {
                    // Use client credentials (service principal)
                    var connString = $"AuthType=ClientSecret;Url={url};ClientId={clientId};ClientSecret={clientSecret}";
                    
                    if (verbose)
                    {
                        Console.WriteLine("Using service principal authentication (ClientId/ClientSecret)");
                        Console.WriteLine($"Client ID: {clientId}");
                    }
                    
                    serviceClient = new ServiceClient(connString);
                }
                else
                {
                    // Use interactive authentication
                    var connString = $"AuthType=OAuth;Url={url};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto";
                    
                    if (verbose)
                    {
                        Console.WriteLine("Using interactive OAuth authentication");
                    }
                    
                    serviceClient = new ServiceClient(connString);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: Exception during connection initialization");
                Console.Error.WriteLine($"Message: {ex.Message}");
                
                if (verbose)
                {
                    Console.Error.WriteLine($"\nFull Exception:");
                    Console.Error.WriteLine(ex.ToString());
                }
                
                Environment.Exit(1);
                return;
            }

            if (!serviceClient.IsReady)
            {
                Console.Error.WriteLine($"Error: Failed to connect to environment");
                Console.Error.WriteLine($"Last Error: {serviceClient.LastError}");
                Console.Error.WriteLine($"Last Exception: {serviceClient.LastException?.Message}");
                
                if (verbose && serviceClient.LastException != null)
                {
                    Console.Error.WriteLine($"\nFull Exception Details:");
                    Console.Error.WriteLine(serviceClient.LastException.ToString());
                }
                
                Environment.Exit(1);
            }

            if (verbose)
            {
                Console.WriteLine($"✓ Connected to {serviceClient.ConnectedOrgFriendlyName}");
            }

            Console.WriteLine("Extracting schema...");
            var extractor = new SchemaExtractor(serviceClient, verbose);
            var schema = await extractor.ExtractSchema(solution);

            // Determine output file name
            string fileName = output;
            if (output == "powerapp-schema.json" && !string.IsNullOrWhiteSpace(solution))
            {
                fileName = $"powerapp-schema-{solution}.json";
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(schema, options);
            await File.WriteAllTextAsync(fileName, json);

            Console.WriteLine($"\n✓ Schema extracted successfully!");
            Console.WriteLine($"Output: {Path.GetFullPath(fileName)}");
            Console.WriteLine($"\nStatistics:");
            Console.WriteLine($"  Entities: {schema.Entities.Count}");
            Console.WriteLine($"  Attributes: {schema.Entities.Sum(e => e.Attributes.Count)}");
            Console.WriteLine($"  Relationships: {schema.Relationships.Count}");
            
            if (!string.IsNullOrWhiteSpace(solution) && schema.SolutionComponents != null)
            {
                Console.WriteLine($"  Solution Components: {schema.SolutionComponents.Count}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            }
            Environment.Exit(1);
        }
    }
}
