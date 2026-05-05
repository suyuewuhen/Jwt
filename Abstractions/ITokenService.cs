using System.Security.Claims;

namespace Jwt.Abstractions;

/// <summary>
/// JWT令牌生成服务
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 生成单个JWT访问令牌
    /// </summary>
    /// <param name="claims">自定义声明</param>
    /// <returns>JWT令牌字符串</returns>
    string BuildToken(IEnumerable<Claim> claims);

    /// <summary>
    /// 生成双令牌（AccessToken + RefreshToken）
    /// </summary>
    /// <param name="claims">自定义声明，必须包含ClaimTypes.NameIdentifier作为用户ID</param>
    /// <param name="deviceId">设备ID，用于多设备登录管控</param>
    /// <returns>访问令牌和刷新令牌元组</returns>
    Task<(string accessToken, string refreshToken)> BuildTokensAsync(IEnumerable<Claim> claims, string deviceId = "");

    /// <summary>
    /// 刷新双令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="deviceId">当前请求设备ID</param>
    /// <returns>新的双令牌，刷新失败返回null</returns>
    Task<(string accessToken, string refreshToken)?> RefreshTokensAsync(string refreshToken, string deviceId = "");

    /// <summary>
    /// 吊销指定刷新令牌（退出登录）
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    Task RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 吊销用户所有刷新令牌（密码修改/安全风控场景）
    /// </summary>
    /// <param name="userId">用户ID</param>
    Task RevokeAllUserRefreshTokensAsync(Guid userId);
}
