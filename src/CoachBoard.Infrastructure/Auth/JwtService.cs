using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoachBoard.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoachBoard.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(int userId, string email, string role, int? coachId = null, int? tenantId = null)
    {
        var claims = new List<Claim>
        {
            new Claim("uid", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (coachId.HasValue)
        {
            claims.Add(new Claim("coachId", coachId.Value.ToString()));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tid", tenantId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
