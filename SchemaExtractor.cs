using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace PowerApps.JsonSchema;

public class SchemaExtractor
{
    private readonly ServiceClient _serviceClient;
    private readonly bool _verbose;

    public SchemaExtractor(ServiceClient serviceClient, bool verbose = false)
    {
        _serviceClient = serviceClient;
        _verbose = verbose;
    }

    public async Task<PowerAppsSchema> ExtractSchema(string? solutionName = null, string? attributePrefix = null, HashSet<string>? excludeAttributes = null)
    {
        var schema = new PowerAppsSchema
        {
            ExtractedDate = DateTime.UtcNow,
            EnvironmentUrl = _serviceClient.ConnectedOrgPublishedEndpoints.ContainsKey(Microsoft.Xrm.Sdk.Discovery.EndpointType.OrganizationService) 
                ? _serviceClient.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.OrganizationService]
                : _serviceClient.ConnectedOrgUriActual?.ToString() ?? string.Empty,
            OrganizationName = _serviceClient.ConnectedOrgFriendlyName,
            SolutionName = solutionName
        };

        // Get all entity metadata
        var request = new RetrieveAllEntitiesRequest
        {
            EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
            RetrieveAsIfPublished = true
        };

        if (_verbose) Console.WriteLine("Retrieving entity metadata...");
        var response = (RetrieveAllEntitiesResponse)await _serviceClient.ExecuteAsync(request);

        // Filter entities if solution specified
        IEnumerable<EntityMetadata> entities = response.EntityMetadata;
        if (!string.IsNullOrWhiteSpace(solutionName))
        {
            var solutionEntities = await GetSolutionEntities(solutionName);
            entities = entities.Where(e => solutionEntities.Contains(e.LogicalName));
            
            schema.SolutionComponents = solutionEntities.ToList();
        }

        var entityList = entities.OrderBy(e => e.LogicalName).ToList();
        int processed = 0;
        
        foreach (var entity in entityList)
        {
            processed++;
            if (_verbose)
            {
                Console.WriteLine($"  [{processed}/{entityList.Count}] {entity.LogicalName}");
            }
            else
            {
                // Show progress for non-verbose mode
                if (processed % 10 == 0 || processed == entityList.Count)
                {
                    Console.Write($"\rProcessing entities: {processed}/{entityList.Count}");
                }
            }
            
            var entitySchema = new EntitySchema
            {
                LogicalName = entity.LogicalName,
                SchemaName = entity.SchemaName,
                DisplayName = entity.DisplayName?.UserLocalizedLabel?.Label,
                PrimaryIdAttribute = entity.PrimaryIdAttribute,
                PrimaryNameAttribute = entity.PrimaryNameAttribute,
                EntitySetName = entity.EntitySetName,
                IsCustomEntity = entity.IsCustomEntity ?? false,
                IsActivity = entity.IsActivity ?? false,
                OwnershipType = entity.OwnershipType?.ToString(),
                Description = entity.Description?.UserLocalizedLabel?.Label
            };

            // Extract attributes
            if (entity.Attributes != null)
            {
                foreach (var attribute in entity.Attributes.OrderBy(a => a.LogicalName))
                {
                    // Apply attribute prefix filter
                    if (!string.IsNullOrWhiteSpace(attributePrefix) && 
                        !attribute.LogicalName.StartsWith(attributePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Apply exclude filter
                    if (excludeAttributes != null && 
                        excludeAttributes.Contains(attribute.LogicalName))
                    {
                        continue;
                    }

                    entitySchema.Attributes.Add(new AttributeSchema
                    {
                        LogicalName = attribute.LogicalName,
                        SchemaName = attribute.SchemaName,
                        DisplayName = attribute.DisplayName?.UserLocalizedLabel?.Label,
                        AttributeType = attribute.AttributeType?.ToString(),
                        IsCustomAttribute = attribute.IsCustomAttribute ?? false,
                        IsPrimaryId = attribute.IsPrimaryId ?? false,
                        IsPrimaryName = attribute.IsPrimaryName ?? false,
                        IsValidForCreate = attribute.IsValidForCreate ?? false,
                        IsValidForUpdate = attribute.IsValidForUpdate ?? false,
                        IsValidForRead = attribute.IsValidForRead ?? false,
                        RequiredLevel = attribute.RequiredLevel?.Value.ToString(),
                        Description = attribute.Description?.UserLocalizedLabel?.Label,
                        MaxLength = (attribute as StringAttributeMetadata)?.MaxLength,
                        Format = (attribute as StringAttributeMetadata)?.Format?.ToString() 
                               ?? (attribute as DateTimeAttributeMetadata)?.Format?.ToString(),
                        MinValue = (attribute as IntegerAttributeMetadata)?.MinValue 
                                ?? (double?)(attribute as DecimalAttributeMetadata)?.MinValue 
                                ?? (attribute as DoubleAttributeMetadata)?.MinValue 
                                ?? (double?)(attribute as MoneyAttributeMetadata)?.MinValue,
                        MaxValue = (attribute as IntegerAttributeMetadata)?.MaxValue 
                                ?? (double?)(attribute as DecimalAttributeMetadata)?.MaxValue 
                                ?? (attribute as DoubleAttributeMetadata)?.MaxValue 
                                ?? (double?)(attribute as MoneyAttributeMetadata)?.MaxValue,
                        Precision = (attribute as DecimalAttributeMetadata)?.Precision 
                                 ?? (attribute as MoneyAttributeMetadata)?.Precision,
                        OptionSet = ExtractOptionSet(attribute),
                        Targets = (attribute as LookupAttributeMetadata)?.Targets
                    });
                }
            }

            schema.Entities.Add(entitySchema);
        }

        if (!_verbose) Console.WriteLine(); // New line after progress

        // Extract relationships
        if (_verbose) Console.WriteLine("\nExtracting relationships...");
        foreach (var entity in entities)
        {
            if (entity.OneToManyRelationships != null)
            {
                foreach (var rel in entity.OneToManyRelationships)
                {
                    schema.Relationships.Add(new RelationshipSchema
                    {
                        SchemaName = rel.SchemaName,
                        RelationshipType = "OneToMany",
                        ReferencingEntity = rel.ReferencingEntity,
                        ReferencingAttribute = rel.ReferencingAttribute,
                        ReferencedEntity = rel.ReferencedEntity,
                        ReferencedAttribute = rel.ReferencedAttribute,
                        IsCustomRelationship = rel.IsCustomRelationship ?? false
                    });
                }
            }

            if (entity.ManyToManyRelationships != null)
            {
                foreach (var rel in entity.ManyToManyRelationships)
                {
                    schema.Relationships.Add(new RelationshipSchema
                    {
                        SchemaName = rel.SchemaName,
                        RelationshipType = "ManyToMany",
                        Entity1LogicalName = rel.Entity1LogicalName,
                        Entity2LogicalName = rel.Entity2LogicalName,
                        IntersectEntityName = rel.IntersectEntityName,
                        IsCustomRelationship = rel.IsCustomRelationship ?? false
                    });
                }
            }
        }

        return schema;
    }

    private async Task<HashSet<string>> GetSolutionEntities(string solutionName)
    {
        if (_verbose) Console.WriteLine($"Filtering by solution: {solutionName}");
        
        var entities = new HashSet<string>();

        // Query for solution
        var solutionQuery = new QueryExpression("solution")
        {
            ColumnSet = new ColumnSet("solutionid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("uniquename", ConditionOperator.Equal, solutionName)
                }
            }
        };

        var solutions = await _serviceClient.RetrieveMultipleAsync(solutionQuery);
        if (solutions.Entities.Count == 0)
        {
            throw new Exception($"Solution '{solutionName}' not found");
        }

        var solutionId = solutions.Entities[0].Id;

        // Query for solution components
        var componentQuery = new QueryExpression("solutioncomponent")
        {
            ColumnSet = new ColumnSet("objectid", "componenttype"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
                    new ConditionExpression("componenttype", ConditionOperator.Equal, 1) // Entity
                }
            }
        };

        var components = await _serviceClient.RetrieveMultipleAsync(componentQuery);

        // Get entity metadata for each component
        foreach (var component in components.Entities)
        {
            var entityId = component.GetAttributeValue<Guid>("objectid");
            
            var metadataRequest = new RetrieveEntityRequest
            {
                MetadataId = entityId,
                EntityFilters = EntityFilters.Entity
            };

            try
            {
                var metadataResponse = (RetrieveEntityResponse)await _serviceClient.ExecuteAsync(metadataRequest);
                entities.Add(metadataResponse.EntityMetadata.LogicalName);
            }
            catch
            {
                // Skip if entity metadata can't be retrieved
            }
        }

        return entities;
    }

    private OptionSetSchema? ExtractOptionSet(AttributeMetadata attribute)
    {
        if (attribute is PicklistAttributeMetadata picklistAttr && picklistAttr.OptionSet != null)
        {
            var optionSet = picklistAttr.OptionSet;
            return new OptionSetSchema
            {
                Name = optionSet.Name,
                IsGlobal = optionSet.IsGlobal ?? false,
                Options = optionSet.Options.Select(o => new OptionSchema
                {
                    Value = o.Value ?? 0,
                    Label = o.Label?.UserLocalizedLabel?.Label
                }).ToList()
            };
        }

        if (attribute is StateAttributeMetadata stateAttr && stateAttr.OptionSet != null)
        {
            return new OptionSetSchema
            {
                Name = stateAttr.OptionSet.Name,
                IsGlobal = false,
                Options = stateAttr.OptionSet.Options.Select(o => new OptionSchema
                {
                    Value = o.Value ?? 0,
                    Label = o.Label?.UserLocalizedLabel?.Label
                }).ToList()
            };
        }

        if (attribute is StatusAttributeMetadata statusAttr && statusAttr.OptionSet != null)
        {
            return new OptionSetSchema
            {
                Name = statusAttr.OptionSet.Name,
                IsGlobal = false,
                Options = statusAttr.OptionSet.Options.Select(o => new OptionSchema
                {
                    Value = o.Value ?? 0,
                    Label = o.Label?.UserLocalizedLabel?.Label
                }).ToList()
            };
        }

        return null;
    }
}
