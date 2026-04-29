using System.Security.Claims;

namespace Jwt.Abstractions;

/// <summary>
/// JWT令牌生成服务
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="claims">自定义声明</param>
    /// <returns>JWT令牌字符串</returns>
    string BuildToken(IEnumerable<Claim> claims);
}
