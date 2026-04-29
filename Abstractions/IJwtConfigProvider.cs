namespace Jwt.Abstractions;

/// <summary>
/// JWT配置提供程序接口
/// </summary>
public interface IJwtConfigProvider
{
    /// <summary>
    /// 同步获取JWT配置
    /// </summary>
    /// <returns>JWT配置选项</returns>
    JwtOptions GetJwtConfig();
    
    /// <summary>
    /// 异步获取JWT配置
    /// </summary>
    /// <returns>JWT配置选项</returns>
    Task<JwtOptions> GetJwtConfigAsync(CancellationToken cancellationToken = default);
}
