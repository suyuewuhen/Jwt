using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jwt.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace Jwt.Implementations;

/// <summary>
/// JWT令牌服务实现
/// </summary>
public class JwtTokenService : ITokenService, IJwtParser
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(JwtOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #region ITokenService 实现
    public string BuildToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_options.ExpireSeconds),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(tokenDescriptor);
    }
    #endregion

    #region IJwtParser 实现
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) 
            return null;

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ClockSkew = TimeSpan.Zero,
            SaveSigninToken = true
        };

        try
        {
            return _tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public Guid GetUserId(string token)
    {
        var user = ValidateToken(token);
        if (user == null)
            return Guid.Empty;

        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public string GetUserName(string token)
    {
        var user = ValidateToken(token);
        if (user == null)
            return string.Empty;

        return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public List<string> GetRoles(string token)
    {
        var user = ValidateToken(token);
        if (user == null)
            return new List<string>();

        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    public IEnumerable<Claim> ParseClaimsUnsafe(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Enumerable.Empty<Claim>();

        try
        {
            var jwt = _tokenHandler.ReadJwtToken(token);
            return jwt.Claims;
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }
    #endregion
}
