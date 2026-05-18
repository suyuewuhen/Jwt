using Jwt.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Jwt.Extensions;

/// <summary>
/// JWT自动续期中间件
/// 当AccessToken即将过期时，自动在响应头返回新的AccessToken和RefreshToken
/// </summary>
public class JwtAutoRenewalMiddleware
{
    private readonly RequestDelegate _next;
    private const string AuthorizationHeader = "Authorization";
    private const string NewAccessTokenHeader = "X-New-Access-Token";
    private const string NewRefreshTokenHeader = "X-New-Refresh-Token";
    private const int RenewalThresholdSeconds = 300; // 剩余5分钟时自动续期

    public JwtAutoRenewalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService, IJwtParser jwtParser)
    {
        var authorizationHeader = context.Request.Headers[AuthorizationHeader].ToString();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            var accessToken = authorizationHeader["Bearer ".Length..].Trim();
            var claimsPrincipal = jwtParser.ValidateToken(accessToken);
            
            if (claimsPrincipal != null)
            {
                // 检查AccessToken是否即将过期
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var remainingSeconds = (jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds;
                
                if (remainingSeconds > 0 && remainingSeconds < RenewalThresholdSeconds)
                {
                    // 尝试获取RefreshToken
                    var refreshToken = context.Request.Headers["X-Refresh-Token"].ToString();
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var deviceId = context.Request.Headers["User-Agent"].ToString();
                        var newTokens = await tokenService.RefreshTokensAsync(refreshToken, deviceId);
                        
                        if (newTokens.HasValue)
                        {
                            // 将新令牌放入响应头
                            context.Response.Headers[NewAccessTokenHeader] = newTokens.Value.accessToken;
                            context.Response.Headers[NewRefreshTokenHeader] = newTokens.Value.refreshToken;
                            
                            // 更新当前请求的ClaimsPrincipal
                            context.User = new ClaimsPrincipal(new ClaimsIdentity(claimsPrincipal.Claims, "JWT"));
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// 自动续期中间件扩展方法
/// </summary>
public static class JwtAutoRenewalMiddlewareExtensions
{
    /// <summary>
    /// 启用JWT自动续期中间件
    /// </summary>
    public static IApplicationBuilder UseJwtAutoRenewal(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtAutoRenewalMiddleware>();
    }
}
