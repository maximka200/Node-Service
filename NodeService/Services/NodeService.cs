using NodeService.Models;
using NodeService.Repository;
using NodeService.Services.Interfaces;

namespace NodeService.Services;

public class NodeService(AppDbContext context) : INodeService
{
    public Task<NodeDto> GetNodeAsync(Guid id)
    {
        var node = context.GetNode(id);
        return node == null ? Task.FromResult<NodeDto>(null!) : Task.FromResult(MapNode(node));
    }

    public Task<IEnumerable<NodeDto>> GetTreeAsync()
    {
        var nodes = context.GetTree();
        var tree = nodes.Select(MapNode);
        return Task.FromResult(tree);
    }

    public Task<NodeDto> CreateNodeAsync(CreateNodeRequest request)
    {
        var node = context.CreateNode(request.Name, request.ParentId);
        return Task.FromResult(MapNode(node));
    }

    public Task<bool> UpdateNodeAsync(Guid id, UpdateNodeRequest request)
    {
        var success = context.UpdateNode(id, request.Name, request.ParentId);
        return Task.FromResult(success);
    }

    public Task<bool> DeleteNodeAsync(Guid id)
    {
        var success = context.DeleteNode(id);
        return Task.FromResult(success);
    }

    private static NodeDto MapNode(Node node)
    {
        return new NodeDto
        {
            Id = node.Id,
            Name = node.Name,
            Children = node.Children?.Select(MapNode).ToList() ?? new List<NodeDto>()
        };
    }
}