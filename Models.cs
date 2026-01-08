namespace PowerApps.JsonSchema;

public class PowerAppsSchema
{
    public DateTime ExtractedDate { get; set; }
    public string EnvironmentUrl { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? SolutionName { get; set; }
    public List<string>? SolutionComponents { get; set; }
    public List<EntitySchema> Entities { get; set; } = new();
    public List<RelationshipSchema> Relationships { get; set; } = new();
}

public class EntitySchema
{
    public string LogicalName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? PrimaryIdAttribute { get; set; }
    public string? PrimaryNameAttribute { get; set; }
    public string? EntitySetName { get; set; }
    public bool IsCustomEntity { get; set; }
    public bool IsActivity { get; set; }
    public string? OwnershipType { get; set; }
    public List<AttributeSchema> Attributes { get; set; } = new();
}

public class AttributeSchema
{
    public string LogicalName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? AttributeType { get; set; }
    public bool IsCustomAttribute { get; set; }
    public bool IsPrimaryId { get; set; }
    public bool IsPrimaryName { get; set; }
    public bool IsValidForCreate { get; set; }
    public bool IsValidForUpdate { get; set; }
    public bool IsValidForRead { get; set; }
    public string? RequiredLevel { get; set; }
    public int? MaxLength { get; set; }
    public string? Format { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? Precision { get; set; }
    public OptionSetSchema? OptionSet { get; set; }
    public string[]? Targets { get; set; }
}

public class OptionSetSchema
{
    public string? Name { get; set; }
    public bool IsGlobal { get; set; }
    public List<OptionSchema> Options { get; set; } = new();
}

public class OptionSchema
{
    public int Value { get; set; }
    public string? Label { get; set; }
}

public class RelationshipSchema
{
    public string SchemaName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    
    // OneToMany properties
    public string? ReferencingEntity { get; set; }
    public string? ReferencingAttribute { get; set; }
    public string? ReferencedEntity { get; set; }
    public string? ReferencedAttribute { get; set; }
    
    // ManyToMany properties
    public string? Entity1LogicalName { get; set; }
    public string? Entity2LogicalName { get; set; }
    public string? IntersectEntityName { get; set; }
    
    public bool IsCustomRelationship { get; set; }
}
