using AMGH.ITInventory.Security.Jwt;
using AMGH.ITInventory.Shared.Contracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AMGH.ITInventory.API.Controllers;

public class DevUser { public string UserName { get; set; } = ""; public string Password { get; set; } = ""; public string DisplayName { get; set; } = ""; public string[] Roles { get; set; } = Array.Empty<string>(); }

/// <summary>
/// Development token endpoint only. Production authenticates against Microsoft Entra ID
/// (MFA enforced by Conditional Access); this endpoint returns 404 outside Development.
/// </summary>
[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwt; private readonly IConfiguration _cfg; private readonly IWebHostEnvironment _env;
    public AuthController(IJwtTokenService jwt, IConfiguration cfg, IWebHostEnvironment env) { _jwt = jwt; _cfg = cfg; _env = env; }

    [HttpPost("token")]
    public ActionResult<ApiResponse<LoginResponse>> Token(LoginRequest req)
    {
        if (!_env.IsDevelopment()) return NotFound();
        var user = (_cfg.GetSection("DevUsers").Get<List<DevUser>>() ?? new()).FirstOrDefault(u =>
            u.UserName == req.UserName && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(u.Password), System.Text.Encoding.UTF8.GetBytes(req.Password)));
        if (user is null) return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid credentials"));
        var (token, exp) = _jwt.Create(user.UserName, user.DisplayName, user.Roles);
        return ApiResponse<LoginResponse>.Ok(new LoginResponse(token, exp, user.DisplayName, user.Roles));
    }
}
