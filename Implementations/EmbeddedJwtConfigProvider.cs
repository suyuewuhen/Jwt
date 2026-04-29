using System.Reflection;
using Jwt.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Jwt.Implementations;

/// <summary>
/// 从嵌入式资源加载JWT配置
/// </summary>
public class EmbeddedJwtConfigProvider : IJwtConfigProvider
{
    private readonly Lazy<JwtOptions> _config;

    public EmbeddedJwtConfigProvider()
    {
        _config = new Lazy<JwtOptions>(LoadConfig);
    }

    public JwtOptions GetJwtConfig() => _config.Value;

    public Task<JwtOptions> GetJwtConfigAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_config.Value);
    }

    private JwtOptions LoadConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.appsettings.json";
        
        var configuration = new ConfigurationBuilder();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            configuration.AddJsonStream(stream);
        }

        var configRoot = configuration.Build();
        var jwtOptions = configRoot.GetSection("JWT").Get<JwtOptions>() ?? new JwtOptions();
        
        ValidateConfig(jwtOptions);
        return jwtOptions;
    }

    private static void ValidateConfig(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey))
            throw new InvalidOperationException("JWT SecretKey 配置不能为空");
        if (string.IsNullOrWhiteSpace(options.Issuer))
            throw new InvalidOperationException("JWT Issuer 配置不能为空");
        if (string.IsNullOrWhiteSpace(options.Audience))
            throw new InvalidOperationException("JWT Audience 配置不能为空");
        if (options.ExpireSeconds <= 0)
            throw new InvalidOperationException("JWT ExpireSeconds 必须大于0");
    }
}
