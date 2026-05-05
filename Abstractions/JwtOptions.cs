namespace Jwt.Abstractions;

/// <summary>
/// JWT配置选项
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 令牌颁发者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// 令牌受众
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// 签名密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// AccessToken过期时间（秒，默认1小时）
    /// </summary>
    public int ExpireSeconds { get; set; } = 3600;

    /// <summary>
    /// RefreshToken过期时间（天，默认7天），优先级低于RefreshTokenExpireSeconds
    /// </summary>
    public int RefreshTokenExpireDays { get; set; } = 7;

    /// <summary>
    /// RefreshToken过期时间（秒，优先级高于RefreshTokenExpireDays）
    /// </summary>
    public int RefreshTokenExpireSeconds { get; set; } = 0;

    /// <summary>
    /// 最终生效的RefreshToken过期时间（秒）
    /// </summary>
    public int FinalRefreshTokenExpireSeconds => RefreshTokenExpireSeconds > 0 ? RefreshTokenExpireSeconds : RefreshTokenExpireDays * 86400;

    /// <summary>
    /// 是否启用单设备登录（默认false，允许多设备同时在线）
    /// </summary>
    public bool SingleDeviceLogin { get; set; } = false;
}
