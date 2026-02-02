namespace NodeService.Models;

public record RegisterRequest(string UserName, string Password);
public record LoginRequest(string UserName, string Password);