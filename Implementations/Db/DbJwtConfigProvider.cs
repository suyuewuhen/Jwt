using Jwt.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Jwt.Implementations.Db;

/// <summary>
/// 从数据库加载JWT配置
/// </summary>
public class DbJwtConfigProvider : IJwtConfigProvider
{
    private readonly JwtDbContext _dbContext;

    public DbJwtConfigProvider(JwtDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public JwtOptions GetJwtConfig()
    {
        var configs = _dbContext.SystemConfigs
            .Where(c => c.ConfigKey.StartsWith("Jwt"))
            .ToList();

        return BuildOptions(configs);
    }

    public async Task<JwtOptions> GetJwtConfigAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _dbContext.SystemConfigs
            .Where(c => c.ConfigKey.StartsWith("Jwt"))
            .ToListAsync(cancellationToken);

        return BuildOptions(configs);
    }

    private JwtOptions BuildOptions(List<SystemConfig> configs)
    {
        var expireMinutes = int.Parse(GetValue(configs, "JwtExpireMinutes", "60"));
        var refreshExpireSeconds = int.Parse(GetValue(configs, "JwtRefreshExpireSeconds", "0"));
        var refreshExpireDays = int.Parse(GetValue(configs, "JwtRefreshExpireDays", "7"));
        var singleDeviceLogin = bool.Parse(GetValue(configs, "JwtSingleDeviceLogin", "false"));
        var enableSlidingExpiration = bool.Parse(GetValue(configs, "JwtEnableSlidingExpiration", "false"));
        var slidingExpirationThreshold = int.Parse(GetValue(configs, "JwtSlidingExpirationThreshold", "86400"));
        var slidingExtendSeconds = int.Parse(GetValue(configs, "JwtSlidingExtendSeconds", "0"));
        var enableRefreshRateLimit = bool.Parse(GetValue(configs, "JwtEnableRefreshRateLimit", "true"));
        var refreshRateLimitCount = int.Parse(GetValue(configs, "JwtRefreshRateLimitCount", "5"));
        var refreshRateLockSeconds = int.Parse(GetValue(configs, "JwtRefreshRateLockSeconds", "600"));
        var enableAccessTokenBlacklist = bool.Parse(GetValue(configs, "JwtEnableAccessTokenBlacklist", "false"));
        var enableHotReload = bool.Parse(GetValue(configs, "JwtEnableHotReload", "false"));
        var hotReloadIntervalSeconds = int.Parse(GetValue(configs, "JwtHotReloadIntervalSeconds", "300"));
        
        return new JwtOptions
        {
            SecretKey = GetValue(configs, "JwtSecretKey"),
            Issuer = GetValue(configs, "JwtIssuer"),
            Audience = GetValue(configs, "JwtAudience"),
            ExpireSeconds = expireMinutes * 60,
            RefreshTokenExpireSeconds = refreshExpireSeconds,
            RefreshTokenExpireDays = refreshExpireDays,
            SingleDeviceLogin = singleDeviceLogin,
            EnableSlidingExpiration = enableSlidingExpiration,
            SlidingExpirationThreshold = slidingExpirationThreshold,
            SlidingExtendSeconds = slidingExtendSeconds,
            EnableRefreshRateLimit = enableRefreshRateLimit,
            RefreshRateLimitCount = refreshRateLimitCount,
            RefreshRateLockSeconds = refreshRateLockSeconds,
            EnableAccessTokenBlacklist = enableAccessTokenBlacklist,
            EnableHotReload = enableHotReload,
            HotReloadIntervalSeconds = hotReloadIntervalSeconds
        };
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <param name="configs">配置列表</param>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    private string GetValue(List<SystemConfig> configs, string key, string defaultValue = "")
    {
        return configs.FirstOrDefault(c => c.ConfigKey == key)?.ConfigValue ?? defaultValue;
    }
}
