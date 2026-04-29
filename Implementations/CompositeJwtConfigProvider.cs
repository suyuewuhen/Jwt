using Jwt.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jwt.Implementations;

/// <summary>
/// 组合配置提供程序，支持多级配置降级
/// </summary>
public class CompositeJwtConfigProvider : IJwtConfigProvider
{
    private readonly IEnumerable<IJwtConfigProvider> _providers;
    private readonly ILogger<CompositeJwtConfigProvider> _logger;

    public CompositeJwtConfigProvider(
        IEnumerable<IJwtConfigProvider> providers,
        ILogger<CompositeJwtConfigProvider>? logger = null)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        if (_providers.Count() == 0)
            throw new ArgumentException("至少需要注册一个 IJwtConfigProvider 实现", nameof(providers));
        _logger = logger ?? NullLogger<CompositeJwtConfigProvider>.Instance;
    }

    public JwtOptions GetJwtConfig()
    {
        foreach (var provider in _providers)
        {
            try
            {
                var config = provider.GetJwtConfig();
                if (!string.IsNullOrWhiteSpace(config.SecretKey))
                {
                    _logger.LogInformation("成功从 {Provider} 加载JWT配置", provider.GetType().Name);
                    return config;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从 {Provider} 加载JWT配置失败，尝试下一个提供程序", provider.GetType().Name);
            }
        }

        throw new InvalidOperationException("所有JWT配置提供程序都加载失败");
    }

    public async Task<JwtOptions> GetJwtConfigAsync(CancellationToken cancellationToken = default)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var config = await provider.GetJwtConfigAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(config.SecretKey))
                {
                    _logger.LogInformation("成功从 {Provider} 加载JWT配置", provider.GetType().Name);
                    return config;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从 {Provider} 加载JWT配置失败，尝试下一个提供程序", provider.GetType().Name);
            }
        }

        throw new InvalidOperationException("所有JWT配置提供程序都加载失败");
    }
}
