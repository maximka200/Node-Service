using NodeService.Models;

namespace NodeService.Services.Interfaces;

public interface INodeService
{
    Task<NodeDto> GetNodeAsync(Guid id);
    Task<IEnumerable<NodeDto>> GetTreeAsync();

    Task<NodeDto> CreateNodeAsync(CreateNodeRequest request);
    Task<bool> UpdateNodeAsync(Guid id, UpdateNodeRequest request);
    Task<bool> DeleteNodeAsync(Guid id);
}