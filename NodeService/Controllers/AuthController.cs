using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodeService.Models;
using NodeService.Services.Interfaces;

namespace NodeService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        var success = authService.Register(request.UserName, request.Password, Roles.User);

        if (!success)
            return BadRequest(new Error("User already exists"));

        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("register-admin")]
    public IActionResult RegisterAdmin(RegisterRequest request)
    {
        var success = authService.Register(request.UserName, request.Password, Roles.Admin);
        if (!success)
            return BadRequest("User already exists");
        return Ok();
    }
    
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var token = authService.Login(request.UserName, request.Password);

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new Error("Invalid username or password"));

        return Ok(new { token });
    }
}
