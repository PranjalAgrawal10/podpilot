using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Configuration;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtSettings">The JWT settings.</param>
    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        this.jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public (string Token, int ExpiresIn) GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        Guid? organizationId = null,
        OrganizationRole? organizationRole = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (organizationId.HasValue)
        {
            claims.Add(new Claim(ApplicationConstants.OrganizationIdClaim, organizationId.Value.ToString()));
        }

        if (organizationRole.HasValue)
        {
            claims.Add(new Claim(
                ApplicationConstants.OrganizationRoleClaim,
                ApplicationConstants.ToRoleName(organizationRole.Value)));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresIn = (int)TimeSpan.FromMinutes(jwtSettings.AccessTokenExpirationMinutes).TotalSeconds;

        return (tokenString, expiresIn);
    }

    /// <inheritdoc />
    public Guid? GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero,
                },
                out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
