using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jwt
{
    public class JWTHelper
    {
        private static readonly JWTOptions options = JWTOptions.Instance;
        public static IEnumerable<Claim> ParseClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims;
        }

        /// <summary>
        /// 获取用户Id
        /// <summary>
        public static Guid GetUserId(string token)
        {
            var user = ValidateToken(token);

            // 2. 拿不到 → 验证失败
            if (user == null)
                return Guid.Empty;
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// 获取用户名
        /// </summary>
        public static string GetUserName(string token)
        {
            var user = ValidateToken(token);
            if (user == null)
                return string.Empty;
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// 获取角色
        /// </summary>
        public static List<string> GetRoles(string token)
        {
            var user = ValidateToken(token);

            // 2. 拿不到 → 验证失败
            if (user == null)
                return new List<string>();
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
        /// <summary>
        /// token验证（验证合法性、过期、签名），成功返回ClaimsPrincipal，失败返回null
        /// </summary>
        public static ClaimsPrincipal? ValidateToken(string token)
        {
            if(token == null) return null;
            var handler = new JwtSecurityTokenHandler();
            if (options.SecretKey == null) return null;
            var keyBytes = Encoding.UTF8.GetBytes(options.SecretKey);
            var param = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = options.Issuer,
                ValidAudience = options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero,

                // 关键：允许读取 JWT（必须加）
                SaveSigninToken = true
            };
            try
            {
                return handler.ValidateToken(token, param, out _);
            }
            catch
            {
                return null;
            }
        }



        ////验证并解析 Token（验证合法性、过期、签名）
        //public static ClaimsPrincipal ValidateToken(
        //string token,
        //string validIssuer,
        //string validAudience,
        //string secretKey)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        //    var parameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidIssuer = validIssuer,
        //        ValidAudience = validAudience,
        //        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        //        ClockSkew = TimeSpan.Zero
        //    };

        //    return tokenHandler.ValidateToken(token, parameters, out _);

        //}
    }
}
