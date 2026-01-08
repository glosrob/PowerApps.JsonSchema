using ClosedXML.Excel;
using PowerApps.JsonSchema;

public class ExcelExporter
{
    private static void CreateSummarySheet(XLWorkbook workbook, PowerAppsSchema schema)
    {
        var worksheet = workbook.Worksheets.Add("Summary");
        workbook.Worksheet(1).Position = 1; // Ensure it's the first sheet

        // Title - use solution name if available, otherwise generic title
        string title = !string.IsNullOrWhiteSpace(schema.SolutionName) 
            ? schema.SolutionName 
            : "PowerApps Schema Export";
        
        worksheet.Cell(1, 1).Value = title;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        // Metadata
        worksheet.Cell(3, 1).Value = "Environment:";
        worksheet.Cell(3, 2).Value = schema.EnvironmentUrl;
        worksheet.Cell(3, 1).Style.Font.Bold = true;

        worksheet.Cell(4, 1).Value = "Organization:";
        worksheet.Cell(4, 2).Value = schema.OrganizationName;
        worksheet.Cell(4, 1).Style.Font.Bold = true;

        worksheet.Cell(5, 1).Value = "Solution:";
        worksheet.Cell(5, 2).Value = schema.SolutionName ?? "(All metadata)";
        worksheet.Cell(5, 1).Style.Font.Bold = true;

        worksheet.Cell(6, 1).Value = "Extracted:";
        worksheet.Cell(6, 2).Value = schema.ExtractedDate.ToString("yyyy-MM-dd HH:mm:ss");
        worksheet.Cell(6, 1).Style.Font.Bold = true;

        worksheet.Cell(8, 1).Value = "Statistics:";
        worksheet.Cell(8, 1).Style.Font.Bold = true;
        worksheet.Cell(8, 1).Style.Font.FontSize = 12;

        worksheet.Cell(9, 1).Value = "Total Entities:";
        worksheet.Cell(9, 2).Value = schema.Entities.Count;
        worksheet.Cell(9, 1).Style.Font.Bold = true;

        worksheet.Cell(10, 1).Value = "Total Attributes:";
        worksheet.Cell(10, 2).Value = schema.Entities.Sum(e => e.Attributes.Count);
        worksheet.Cell(10, 1).Style.Font.Bold = true;

        worksheet.Cell(11, 1).Value = "Total Relationships:";
        worksheet.Cell(11, 2).Value = schema.Relationships.Count;
        worksheet.Cell(11, 1).Style.Font.Bold = true;

        // Entity List
        int startRow = 13;
        worksheet.Cell(startRow, 1).Value = "Entities";
        worksheet.Cell(startRow, 1).Style.Font.Bold = true;
        worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;

        // Headers
        int headerRow = startRow + 1;
        worksheet.Cell(headerRow, 1).Value = "Logical Name";
        worksheet.Cell(headerRow, 2).Value = "Display Name";
        worksheet.Cell(headerRow, 3).Value = "Attributes";
        worksheet.Cell(headerRow, 4).Value = "Is Custom";
        worksheet.Cell(headerRow, 5).Value = "Is Activity";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Entity data
        int row = headerRow + 1;
        foreach (var entity in schema.Entities.OrderBy(e => e.LogicalName))
        {
            worksheet.Cell(row, 1).Value = entity.LogicalName;
            worksheet.Cell(row, 2).Value = entity.DisplayName ?? "";
            worksheet.Cell(row, 3).Value = entity.Attributes.Count;
            worksheet.Cell(row, 4).Value = entity.IsCustomEntity ? "Yes" : "No";
            worksheet.Cell(row, 5).Value = entity.IsActivity ? "Yes" : "No";
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }

    public static void ExportToExcel(PowerAppsSchema schema, string outputPath)
    {
        using var workbook = new XLWorkbook();

        // Create summary sheet first
        CreateSummarySheet(workbook, schema);

        // Create one sheet per entity
        foreach (var entity in schema.Entities.OrderBy(e => e.LogicalName))
        {
            CreateEntitySheet(workbook, entity);
        }

        // Add Option Sets sheet at the end
        CreateOptionSetsSheet(workbook, schema);

        workbook.SaveAs(outputPath);
    }

    private static void CreateEntitySheet(XLWorkbook workbook, EntitySchema entity)
    {
        // Sanitize sheet name (Excel has 31 char limit and doesn't allow certain characters)
        var sheetName = entity.LogicalName;
        if (sheetName.Length > 31)
            sheetName = sheetName.Substring(0, 31);
        
        sheetName = sheetName.Replace(":", "_").Replace("/", "_").Replace("\\", "_")
                             .Replace("?", "_").Replace("*", "_").Replace("[", "_").Replace("]", "_");
        
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Entity Information Header
        worksheet.Cell(1, 1).Value = "Entity Information";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        
        worksheet.Cell(2, 1).Value = "Logical Name:";
        worksheet.Cell(2, 2).Value = entity.LogicalName;
        worksheet.Cell(2, 1).Style.Font.Bold = true;
        
        worksheet.Cell(3, 1).Value = "Display Name:";
        worksheet.Cell(3, 2).Value = entity.DisplayName ?? "";
        worksheet.Cell(3, 1).Style.Font.Bold = true;
        
        worksheet.Cell(4, 1).Value = "Schema Name:";
        worksheet.Cell(4, 2).Value = entity.SchemaName;
        worksheet.Cell(4, 1).Style.Font.Bold = true;
        
        worksheet.Cell(5, 1).Value = "Is Custom Entity:";
        worksheet.Cell(5, 2).Value = entity.IsCustomEntity ? "Yes" : "No";
        worksheet.Cell(5, 1).Style.Font.Bold = true;
        
        worksheet.Cell(6, 1).Value = "Is Activity:";
        worksheet.Cell(6, 2).Value = entity.IsActivity ? "Yes" : "No";
        worksheet.Cell(6, 1).Style.Font.Bold = true;
        
        worksheet.Cell(7, 1).Value = "Primary ID:";
        worksheet.Cell(7, 2).Value = entity.PrimaryIdAttribute ?? "";
        worksheet.Cell(7, 1).Style.Font.Bold = true;
        
        worksheet.Cell(8, 1).Value = "Primary Name:";
        worksheet.Cell(8, 2).Value = entity.PrimaryNameAttribute ?? "";
        worksheet.Cell(8, 1).Style.Font.Bold = true;
        
        worksheet.Cell(9, 1).Value = "Ownership Type:";
        worksheet.Cell(9, 2).Value = entity.OwnershipType ?? "";
        worksheet.Cell(9, 1).Style.Font.Bold = true;
        
        worksheet.Cell(10, 1).Value = "Description:";
        worksheet.Cell(10, 2).Value = entity.Description ?? "";
        worksheet.Cell(10, 1).Style.Font.Bold = true;

        // Attributes Section
        int startRow = 12;
        worksheet.Cell(startRow, 1).Value = "Attributes";
        worksheet.Cell(startRow, 1).Style.Font.Bold = true;
        worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;

        // Attribute Headers
        int headerRow = startRow + 1;
        worksheet.Cell(headerRow, 1).Value = "Logical Name";
        worksheet.Cell(headerRow, 2).Value = "Display Name";
        worksheet.Cell(headerRow, 3).Value = "Type";
        worksheet.Cell(headerRow, 4).Value = "Description";
        worksheet.Cell(headerRow, 5).Value = "Required";
        worksheet.Cell(headerRow, 6).Value = "Is Custom";
        worksheet.Cell(headerRow, 7).Value = "Max Length";
        worksheet.Cell(headerRow, 8).Value = "Format";
        worksheet.Cell(headerRow, 9).Value = "Min Value";
        worksheet.Cell(headerRow, 10).Value = "Max Value";
        worksheet.Cell(headerRow, 11).Value = "Targets";

        var attrHeaderRange = worksheet.Range(headerRow, 1, headerRow, 11);
        attrHeaderRange.Style.Font.Bold = true;
        attrHeaderRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Attribute Data
        int row = headerRow + 1;
        foreach (var attr in entity.Attributes.OrderBy(a => a.LogicalName))
        {
            worksheet.Cell(row, 1).Value = attr.LogicalName;
            worksheet.Cell(row, 2).Value = attr.DisplayName ?? "";
            worksheet.Cell(row, 3).Value = attr.AttributeType ?? "";
            worksheet.Cell(row, 4).Value = attr.Description ?? "";
            worksheet.Cell(row, 5).Value = attr.RequiredLevel ?? "";
            worksheet.Cell(row, 6).Value = attr.IsCustomAttribute ? "Yes" : "No";
            worksheet.Cell(row, 7).Value = attr.MaxLength?.ToString() ?? "";
            worksheet.Cell(row, 8).Value = attr.Format ?? "";
            worksheet.Cell(row, 9).Value = attr.MinValue?.ToString() ?? "";
            worksheet.Cell(row, 10).Value = attr.MaxValue?.ToString() ?? "";
            worksheet.Cell(row, 11).Value = attr.Targets != null ? string.Join(", ", attr.Targets) : "";
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }

    private static void CreateOptionSetsSheet(XLWorkbook workbook, PowerAppsSchema schema)
    {
        var worksheet = workbook.Worksheets.Add("Option Sets");

        // Headers
        worksheet.Cell(1, 1).Value = "Entity";
        worksheet.Cell(1, 2).Value = "Attribute";
        worksheet.Cell(1, 3).Value = "Option Set Name";
        worksheet.Cell(1, 4).Value = "Is Global";
        worksheet.Cell(1, 5).Value = "Value";
        worksheet.Cell(1, 6).Value = "Label";

        // Format headers
        var headerRow = worksheet.Range(1, 1, 1, 6);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Data - Extract option sets from attributes
        int row = 2;
        foreach (var entity in schema.Entities.OrderBy(e => e.LogicalName))
        {
            foreach (var attr in entity.Attributes.Where(a => a.OptionSet != null).OrderBy(a => a.LogicalName))
            {
                bool firstOption = true;
                foreach (var option in attr.OptionSet!.Options.OrderBy(o => o.Value))
                {
                    worksheet.Cell(row, 1).Value = entity.LogicalName;
                    worksheet.Cell(row, 2).Value = attr.LogicalName;
                    if (firstOption)
                    {
                        worksheet.Cell(row, 3).Value = attr.OptionSet.Name ?? "";
                        worksheet.Cell(row, 4).Value = attr.OptionSet.IsGlobal ? "Yes" : "No";
                        firstOption = false;
                    }
                    worksheet.Cell(row, 5).Value = option.Value;
                    worksheet.Cell(row, 6).Value = option.Label ?? "";
                    row++;
                }
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }

    public static void ExportToCsv(PowerAppsSchema schema, string outputPath)
    {
        // For CSV, create a single flattened view focusing on entities and their attributes
        var lines = new List<string>();
        
        // Header
        lines.Add("Entity,Attribute,Display Name,Type,Description,Required Level,Is Custom,Max Length,Format,Targets");

        // Data
        foreach (var entity in schema.Entities.OrderBy(e => e.LogicalName))
        {
            foreach (var attr in entity.Attributes.OrderBy(a => a.LogicalName))
            {
                var line = $"\"{entity.LogicalName}\"," +
                          $"\"{attr.LogicalName}\"," +
                          $"\"{EscapeCsv(attr.DisplayName ?? "")}\"," +
                          $"\"{attr.AttributeType ?? ""}\"," +
                          $"\"{EscapeCsv(attr.Description ?? "")}\"," +
                          $"\"{attr.RequiredLevel ?? ""}\"," +
                          $"\"{(attr.IsCustomAttribute ? "Yes" : "No")}\"," +
                          $"\"{attr.MaxLength?.ToString() ?? ""}\"," +
                          $"\"{attr.Format ?? ""}\"," +
                          $"\"{(attr.Targets != null ? string.Join("; ", attr.Targets) : "")}\"";
                lines.Add(line);
            }
        }

        File.WriteAllLines(outputPath, lines);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
