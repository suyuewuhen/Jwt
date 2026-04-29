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
        var expireMinutes = int.Parse(GetValue(configs, "JwtExpireMinutes", "1440"));
        
        return new JwtOptions
        {
            SecretKey = GetValue(configs, "JwtSecretKey"),
            Issuer = GetValue(configs, "JwtIssuer"),
            Audience = GetValue(configs, "JwtAudience"),
            ExpireSeconds = expireMinutes * 60
        };
    }

    private string GetValue(List<SystemConfig> configs, string key, string defaultValue = "")
    {
        return configs.FirstOrDefault(c => c.ConfigKey == key)?.ConfigValue ?? defaultValue;
    }
}
