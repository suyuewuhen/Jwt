namespace Jwt.Abstractions;

/// <summary>
/// JWT配置选项
/// </summary>
public class JwtOptions
{
    #region 基础配置
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
    #endregion

    #region 滑动过期配置
    /// <summary>
    /// 是否启用RefreshToken滑动过期（默认false）
    /// </summary>
    public bool EnableSlidingExpiration { get; set; } = false;

    /// <summary>
    /// 滑动过期阈值：当RefreshToken剩余有效期小于该值时自动续期（秒，默认86400=1天）
    /// </summary>
    public int SlidingExpirationThreshold { get; set; } = 86400;

    /// <summary>
    /// 滑动续期时长：续期后的RefreshToken有效期（秒，默认和初始有效期一致）
    /// </summary>
    public int SlidingExtendSeconds { get; set; } = 0;

    /// <summary>
    /// 最终生效的滑动续期时长
    /// </summary>
    public int FinalSlidingExtendSeconds => SlidingExtendSeconds > 0 ? SlidingExtendSeconds : FinalRefreshTokenExpireSeconds;
    #endregion

    #region 限流配置
    /// <summary>
    /// 是否启用刷新令牌限流（默认true）
    /// </summary>
    public bool EnableRefreshRateLimit { get; set; } = true;

    /// <summary>
    /// 刷新失败最大次数（默认5次）
    /// </summary>
    public int RefreshRateLimitCount { get; set; } = 5;

    /// <summary>
    /// 刷新失败锁定时长（秒，默认600=10分钟）
    /// </summary>
    public int RefreshRateLockSeconds { get; set; } = 600;
    #endregion

    #region 黑名单配置
    /// <summary>
    /// 是否启用AccessToken黑名单（默认false）
    /// </summary>
    public bool EnableAccessTokenBlacklist { get; set; } = false;

    /// <summary>
    /// 黑名单缓存前缀
    /// </summary>
    public string BlacklistPrefix { get; set; } = "jwt:blacklist:";
    #endregion

    #region 热更新配置
    /// <summary>
    /// 是否启用配置热更新（默认false）
    /// </summary>
    public bool EnableHotReload { get; set; } = false;

    /// <summary>
    /// 热更新检查间隔（秒，默认300=5分钟）
    /// </summary>
    public int HotReloadIntervalSeconds { get; set; } = 300;
    #endregion
}
