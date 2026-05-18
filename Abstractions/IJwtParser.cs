using System.Security.Claims;

namespace Jwt.Abstractions;

/// <summary>
/// JWT令牌解析服务
/// </summary>
public interface IJwtParser
{
    /// <summary>
    /// 验证并解析令牌
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>验证通过返回ClaimsPrincipal，失败返回null</returns>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// 从令牌中获取用户ID
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>用户ID，失败返回Guid.Empty</returns>
    Guid GetUserId(string token);
    
    /// <summary>
    /// 从令牌中获取用户名
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>用户名，失败返回空字符串</returns>
    string GetUserName(string token);
    
    /// <summary>
    /// 从令牌中获取用户角色列表
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>角色列表，失败返回空集合</returns>
    List<string> GetRoles(string token);
    
    /// <summary>
    /// 解析令牌中的所有声明（不验证签名）
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>声明集合</returns>
    IEnumerable<Claim> ParseClaimsUnsafe(string token);

    /// <summary>
    /// 从令牌中获取用户手机号
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>手机号，失败返回空字符串</returns>
    string GetUserPhone(string token);

    /// <summary>
    /// 从令牌中获取用户邮箱
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>邮箱，失败返回空字符串</returns>
    string GetUserEmail(string token);

    /// <summary>
    /// 从令牌中获取租户ID
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <returns>租户ID，失败返回Guid.Empty</returns>
    Guid GetTenantId(string token);

    /// <summary>
    /// 从令牌中获取指定类型的声明值
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <param name="claimType">声明类型</param>
    /// <returns>声明值，失败返回空字符串</returns>
    string GetClaimValue(string token, string claimType);

    /// <summary>
    /// 检查AccessToken是否在黑名单中
    /// </summary>
    /// <param name="token">AccessToken</param>
    /// <returns>是否在黑名单中</returns>
    Task<bool> IsTokenBlacklistedAsync(string token);

    /// <summary>
    /// 将AccessToken加入黑名单
    /// </summary>
    /// <param name="token">AccessToken</param>
    /// <param name="expireTime">令牌自然过期时间</param>
    Task AddToBlacklistAsync(string token, DateTime expireTime);
}
