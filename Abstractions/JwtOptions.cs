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
    /// 令牌过期时间（秒）
    /// </summary>
    public int ExpireSeconds { get; set; }
}
