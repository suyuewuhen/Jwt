namespace Jwt.Implementations.Db;

/// <summary>
/// 系统配置表实体
/// </summary>
public class SystemConfig
{
    /// <summary>
    /// 配置键
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;
    
    /// <summary>
    /// 配置值
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;
}
