namespace NodeService.Models;

public record NodeDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public List<NodeDto> Children { get; init; } = [];
}

public record CreateNodeRequest(string Name, Guid? ParentId);
public record UpdateNodeRequest(string Name, Guid? ParentId);