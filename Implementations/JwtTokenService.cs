using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jwt.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace Jwt.Implementations;

/// <summary>
/// 刷新令牌存储信息
/// </summary>
internal class RefreshTokenInfo
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime ExpireTime { get; set; }
    public List<Claim> Claims { get; set; } = [];
}

/// <summary>
/// JWT令牌服务实现
/// </summary>
public class JwtTokenService : ITokenService, IJwtParser
{
    private readonly JwtOptions _options;
    private readonly IDistributedCache _cache;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private const string RefreshTokenPrefix = "jwt:refresh:";
    private const string UserRefreshTokensPrefix = "jwt:user:tokens:";

    public JwtTokenService(JwtOptions options, IDistributedCache cache)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

    /// <summary>
    ///  生成RefreshToken
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<(string accessToken, string refreshToken)> BuildTokensAsync(IEnumerable<Claim> claims, string deviceId = "")
    {
        var claimsList = claims.ToList();
        var accessToken = BuildToken(claimsList);
        
        // 从声明中获取用户ID
        var userIdClaim = claimsList.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("生成双令牌必须包含ClaimTypes.NameIdentifier类型的用户ID声明");
        }

        // 生成高熵RefreshToken
        var refreshToken = GenerateRefreshToken();
        var expireTime = DateTime.UtcNow.AddSeconds(_options.FinalRefreshTokenExpireSeconds);
        
        // 存储RefreshToken信息
        var tokenInfo = new RefreshTokenInfo
        {
            UserId = userId,
            DeviceId = deviceId,
            ExpireTime = expireTime,
            Claims = claimsList
        };

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expireTime
        };

        // 存储刷新令牌
        await _cache.SetStringAsync(
            $"{RefreshTokenPrefix}{refreshToken}", 
            JsonSerializer.Serialize(tokenInfo), 
            cacheOptions);

        // 如果启用单设备登录，吊销用户其他设备的令牌
        if (_options.SingleDeviceLogin)
        {
            await RevokeAllUserRefreshTokensAsync(userId);
        }
        else
        {
            // 存储用户的刷新令牌索引，用于批量吊销
            var userTokensKey = $"{UserRefreshTokensPrefix}{userId}";
            var userTokens = await _cache.GetStringAsync(userTokensKey);
            var tokenList = userTokens != null 
                ? JsonSerializer.Deserialize<List<string>>(userTokens) ?? new List<string>() 
                : new List<string>();
            
            tokenList.Add(refreshToken);
            await _cache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(tokenList), cacheOptions);
        }

        return (accessToken, refreshToken);
    }

    /// <summary>
    /// 刷新双令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="deviceId">当前请求设备ID</param>
    /// <returns>新的双令牌，刷新失败返回null</returns>
    public async Task<(string accessToken, string refreshToken)?> RefreshTokensAsync(string refreshToken, string deviceId = "")
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var tokenKey = $"{RefreshTokenPrefix}{refreshToken}";
        var tokenJson = await _cache.GetStringAsync(tokenKey);
        if (string.IsNullOrWhiteSpace(tokenJson))
            return null;

        var tokenInfo = JsonSerializer.Deserialize<RefreshTokenInfo>(tokenJson);
        if (tokenInfo == null || tokenInfo.ExpireTime < DateTime.UtcNow)
            return null;

        // 验证设备ID匹配
        if (!string.Equals(tokenInfo.DeviceId, deviceId, StringComparison.Ordinal))
            return null;

        // 删除旧的刷新令牌（一次性使用）
        await _cache.RemoveAsync(tokenKey);

        // 移除用户令牌索引中的旧令牌
        if (!_options.SingleDeviceLogin)
        {
            var userTokensKey = $"{UserRefreshTokensPrefix}{tokenInfo.UserId}";
            var userTokensJson = await _cache.GetStringAsync(userTokensKey);
            if (userTokensJson != null)
            {
                var userTokens = JsonSerializer.Deserialize<List<string>>(userTokensJson);
                if (userTokens != null)
                {
                    userTokens.Remove(refreshToken);
                    await _cache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(userTokens));
                }
            }
        }

        // 生成新的双令牌
        return await BuildTokensAsync(tokenInfo.Claims, deviceId);
    }

    /// <summary>
    /// 吊销刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <returns></returns>
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var tokenKey = $"{RefreshTokenPrefix}{refreshToken}";
        var tokenJson = await _cache.GetStringAsync(tokenKey);
        if (string.IsNullOrWhiteSpace(tokenJson))
            return;

        var tokenInfo = JsonSerializer.Deserialize<RefreshTokenInfo>(tokenJson);
        await _cache.RemoveAsync(tokenKey);

        // 移除用户令牌索引中的对应令牌
        if (tokenInfo != null && !_options.SingleDeviceLogin)
        {
            var userTokensKey = $"{UserRefreshTokensPrefix}{tokenInfo.UserId}";
            var userTokensJson = await _cache.GetStringAsync(userTokensKey);
            if (userTokensJson != null)
            {
                var userTokens = JsonSerializer.Deserialize<List<string>>(userTokensJson);
                if (userTokens != null)
                {
                    userTokens.Remove(refreshToken);
                    await _cache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(userTokens));
                }
            }
        }
    }

    /// <summary>
    /// 吊销用户所有刷新令牌（密码修改/安全风控场景）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        var userTokensKey = $"{UserRefreshTokensPrefix}{userId}";
        var userTokensJson = await _cache.GetStringAsync(userTokensKey);
        if (userTokensJson == null)
            return;

        var userTokens = JsonSerializer.Deserialize<List<string>>(userTokensJson);
        if (userTokens == null)
            return;

        // 删除用户所有的刷新令牌
        foreach (var token in userTokens)
        {
            await _cache.RemoveAsync($"{RefreshTokenPrefix}{token}");
        }

        // 删除用户令牌索引
        await _cache.RemoveAsync(userTokensKey);
    }

    /// <summary>
    /// 生成高熵刷新令牌（32位随机字符串）
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
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
