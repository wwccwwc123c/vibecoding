using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vibecoding.Api.Data;
using Vibecoding.Api.Models;
using Vibecoding.Api.Services;

namespace Vibecoding.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _passwordHasher;
    private readonly IJwtService _jwt;

    public AuthController(AppDbContext db, PasswordHasher<AppUser> passwordHasher, IJwtService jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.UserName) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest("用户名和密码不能为空。");

        var name = input.UserName.Trim();
        if (name.Length > 64 || input.Password.Length < 6)
            return BadRequest("用户名过长或密码至少 6 位。");

        if (await _db.Users.AnyAsync(u => u.UserName == name))
            return Conflict("用户名已存在。");

        var user = new AppUser { UserName = name, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "注册成功，请登录。" });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] AuthRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.UserName) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest("用户名和密码不能为空。");

        var name = input.UserName.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == name);
        if (user == null)
            return Unauthorized("用户名或密码错误。");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized("用户名或密码错误。");

        var token = _jwt.CreateToken(user);
        return Ok(new LoginResponse { Token = token, UserName = user.UserName });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new { userName = User.Identity?.Name });
    }

    public class AuthRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
