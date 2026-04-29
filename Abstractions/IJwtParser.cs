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
}
