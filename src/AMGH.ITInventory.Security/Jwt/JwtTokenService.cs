using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AMGH.ITInventory.Security.Jwt;

public class JwtOptions
{
    public string Issuer { get; set; } = "AMGH.ITInventory";
    public string Audience { get; set; } = "AMGH.ITInventory.Clients";
    /// <summary>Must be supplied via secret store / environment; at least 32 chars.</summary>
    public string SigningKey { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 60;
}

public interface IJwtTokenService { (string Token, DateTime ExpiresUtc) Create(string userId, string displayName, IEnumerable<string> roles); }

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _o;
    public JwtTokenService(IOptions<JwtOptions> o) => _o = o.Value;

    public (string, DateTime) Create(string userId, string displayName, IEnumerable<string> roles)
    {
        var expires = DateTime.UtcNow.AddMinutes(_o.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId), new(ClaimTypes.Name, displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey));
        var jwt = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddAmghSecurity(this IServiceCollection services, JwtOptions jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be configured with at least 32 characters.");
        services.AddSingleton<IOptions<JwtOptions>>(Options.Create(jwt));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience,
                ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
            });
        services.AddAuthorization(o =>
        {
            o.AddPolicy(Policies.ManageAssets, p => p.RequireRole(Roles.Admin, Roles.ITManager, Roles.Technician));
            o.AddPolicy(Policies.RunVerification, p => p.RequireRole(Roles.Admin, Roles.ITManager, Roles.Technician, Roles.Auditor, Roles.Custodian));
            o.AddPolicy(Policies.ApproveVerification, p => p.RequireRole(Roles.Admin, Roles.ITManager, Roles.Approver));
            o.AddPolicy(Policies.ViewReports, p => p.RequireRole(Roles.Admin, Roles.ITManager, Roles.Auditor));
            o.AddPolicy(Policies.Administer, p => p.RequireRole(Roles.Admin));
        });
        return services;
    }
}
