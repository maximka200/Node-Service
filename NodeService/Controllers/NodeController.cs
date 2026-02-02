using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodeService.Models;
using NodeService.Services.Interfaces;

namespace NodeService.Controllers;

[ApiController]
[Route("api/nodes")]
public class NodeController(INodeService nodeService) : ControllerBase
{
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var tree = await nodeService.GetTreeAsync();
        return Ok(tree);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNode(Guid id)
    {
        try
        {
            var node = await nodeService.GetNodeAsync(id);
            return Ok(node);
        }
        catch
        {
            return BadRequest(new Error("Cannot get node (not found)"));
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateNode(CreateNodeRequest request)
    {
        try
        {
            var node = await nodeService.CreateNodeAsync(request);
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, node);
        }
        catch 
        {
            return BadRequest(new Error("Cannot create node (parent not found)"));
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateNode(Guid id, UpdateNodeRequest request)
    {
        var success = await nodeService.UpdateNodeAsync(id, request);
        if (!success) return BadRequest(new Error("Cannot update node (cycle or not found)"));
        return NoContent();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNode(Guid id)
    {
        var success = await nodeService.DeleteNodeAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
