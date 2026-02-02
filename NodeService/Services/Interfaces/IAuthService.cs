using NodeService.Models;

namespace NodeService.Services.Interfaces;

public interface IAuthService
{
    public string Login(string username, string password);
    public bool Register(string username, string password, Roles role);
}