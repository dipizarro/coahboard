using BCrypt.Net;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoachBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly ICoachRepository _coaches;
    private readonly ITenantRepository _tenants;
    private readonly IJwtService _jwt;

    public AuthController(IUserRepository users, ICoachRepository coaches, ITenantRepository tenants, IJwtService jwt)
    {
        _users = users;
        _coaches = coaches;
        _tenants = tenants;
        _jwt = jwt;
    }

    /// <summary>
    /// Registra un nuevo usuario y su perfil de Coach.
    /// </summary>
    /// <param name="req">Datos de registro.</param>
    /// <returns>Respuesta de autenticación con token JWT.</returns>
    [HttpPost("register")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var existing = await _users.GetByEmailAsync(req.Email.Trim().ToLower());
        if (existing is not null) return Conflict("Email ya registrado.");

        var normalizedEmail = req.Email.Trim().ToLower();
        var role = string.IsNullOrWhiteSpace(req.Role) ? "Coach" : req.Role.Trim();

        // 1. Create Tenant
        // For now, tenant name = Coach's name or a default.
        var tenantName = string.IsNullOrWhiteSpace(req.Name) ? "My Workspace" : $"{req.Name}'s Workspace";
        var tenant = new Tenant { Name = tenantName };
        await _tenants.AddAsync(tenant);
        await _tenants.SaveChangesAsync(); // Get ID

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role,
            TenantId = tenant.Id
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        int? coachId = null;

        if (role.Equals("Coach", StringComparison.OrdinalIgnoreCase))
        {
            var name = string.IsNullOrWhiteSpace(req.Name)
                ? normalizedEmail.Split('@')[0]
                : req.Name.Trim();

            var specialty = string.IsNullOrWhiteSpace(req.Specialty)
                ? "General"
                : req.Specialty.Trim();

            var coach = new Coach
            {
                UserId = user.Id,
                Name = name,
                Specialty = specialty,
                TenantId = tenant.Id
            };

            await _coaches.AddAsync(coach);
            await _coaches.SaveChangesAsync();

            coachId = coach.Id;
        }

        var token = _jwt.GenerateToken(user.Email, user.Role, coachId, tenant.Id);
        return Ok(new AuthResponse(token, user.Email, user.Role, coachId));
    }

    /// <summary>
    /// Inicia sesión y obtiene un token JWT.
    /// </summary>
    /// <param name="req">Credenciales de acceso.</param>
    /// <returns>Respuesta de autenticación con token JWT.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var normalizedEmail = req.Email.Trim().ToLower();
        var user = await _users.GetByEmailAsync(normalizedEmail);
        if (user is null) return Unauthorized("Credenciales inválidas.");

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized("Credenciales inválidas.");

        int? coachId = null;
        if (user.Role.Equals("Coach", StringComparison.OrdinalIgnoreCase))
        {
            var coach = await _coaches.GetByUserIdAsync(user.Id);
            coachId = coach?.Id;
        }

        var token = _jwt.GenerateToken(user.Email, user.Role, coachId, user.TenantId);
        return Ok(new AuthResponse(token, user.Email, user.Role, coachId));
    }
}
