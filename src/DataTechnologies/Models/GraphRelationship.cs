namespace DataTechnologies.Models;

public class GraphRelationship
{
    public required string FromLabel { get; set; }
    public required string FromName { get; set; }
    public required string RelationshipType { get; set; }
    public required string ToLabel { get; set; }
    public required string ToName { get; set; }
}
