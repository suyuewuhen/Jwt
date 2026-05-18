using System.Security.Claims;

namespace Jwt.Abstractions;

/// <summary>
/// JWT操作审计日志接口
/// </summary>
public interface IJwtAuditLogger
{
    /// <summary>
    /// 记录令牌生成事件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="deviceId">设备ID</param>
    /// <param name="claims">令牌声明</param>
    Task LogTokenGeneratedAsync(Guid userId, string deviceId, IEnumerable<Claim> claims);

    /// <summary>
    /// 记录令牌刷新事件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="deviceId">设备ID</param>
    /// <param name="oldRefreshToken">旧刷新令牌</param>
    Task LogTokenRefreshedAsync(Guid userId, string deviceId, string oldRefreshToken);

    /// <summary>
    /// 记录令牌吊销事件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="refreshToken">被吊销的刷新令牌</param>
    /// <param name="reason">吊销原因</param>
    Task LogTokenRevokedAsync(Guid userId, string refreshToken, string reason = "");

    /// <summary>
    /// 记录用户全量令牌吊销事件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="reason">吊销原因</param>
    Task LogAllUserTokensRevokedAsync(Guid userId, string reason = "");

    /// <summary>
    /// 记录刷新令牌验证失败事件
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="deviceId">设备ID</param>
    /// <param name="failReason">失败原因</param>
    Task LogRefreshTokenFailedAsync(string refreshToken, string deviceId, string failReason);
}
