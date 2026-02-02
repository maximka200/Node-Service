using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodeService.Models;
using NodeService.Repository;
using NodeService.Services.Interfaces;

namespace NodeService.Services;

public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    public bool Register(string username, string password, Roles role)
    {
        return context.CreateUser(username, password, role);
    }

    public string Login(string username, string password)
    {
        var user = context.GetUser(username);

        if (user == null)
            return string.Empty;

        var validPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return !validPassword ? string.Empty : GenerateJwtToken(user);
    }
    
    private string GenerateJwtToken(User user)
    {
        var jwt = configuration.GetSection("Jwt");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"] ?? string.Empty)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}