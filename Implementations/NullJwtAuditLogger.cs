using Jwt.Abstractions;
using System.Security.Claims;

namespace Jwt.Implementations;

/// <summary>
/// 空审计日志实现（默认实现，不做任何操作）
/// </summary>
internal class NullJwtAuditLogger : IJwtAuditLogger
{
    public Task LogTokenGeneratedAsync(Guid userId, string deviceId, IEnumerable<Claim> claims) => Task.CompletedTask;

    public Task LogTokenRefreshedAsync(Guid userId, string deviceId, string oldRefreshToken) => Task.CompletedTask;

    public Task LogTokenRevokedAsync(Guid userId, string refreshToken, string reason = "") => Task.CompletedTask;

    public Task LogAllUserTokensRevokedAsync(Guid userId, string reason = "") => Task.CompletedTask;

    public Task LogRefreshTokenFailedAsync(string refreshToken, string deviceId, string failReason) => Task.CompletedTask;
}
