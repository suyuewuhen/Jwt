using Jwt.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jwt.Implementations;

/// <summary>
/// JWT配置热更新后台服务
/// </summary>
internal class JwtConfigHotReloadService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly JwtOptions _currentOptions;
    private readonly int _checkIntervalSeconds;

    public JwtConfigHotReloadService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<JwtOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _currentOptions = options.Value;
        _checkIntervalSeconds = _currentOptions.HotReloadIntervalSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_currentOptions.EnableHotReload)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var configProvider = scope.ServiceProvider.GetRequiredService<IJwtConfigProvider>();
                var newConfig = await configProvider.GetJwtConfigAsync(stoppingToken);

                // 配置有变化时更新当前配置
                if (newConfig != null)
                {
                    // 反射更新配置属性
                    var properties = typeof(JwtOptions).GetProperties();
                    foreach (var prop in properties.Where(p => p.CanWrite))
                    {
                        var newValue = prop.GetValue(newConfig);
                        var oldValue = prop.GetValue(_currentOptions);
                        if (newValue != null && !newValue.Equals(oldValue))
                        {
                            prop.SetValue(_currentOptions, newValue);
                        }
                    }
                }
            }
            catch
            {
                // 忽略热更新错误，继续使用原有配置
            }

            await Task.Delay(TimeSpan.FromSeconds(_checkIntervalSeconds), stoppingToken);
        }
    }
}
