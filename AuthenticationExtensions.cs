using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jwt
{
    public static class AuthenticationExtensions
    {
        //AddAuthenticationConfig
        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services)
        {
            JWTOptions jwtOpt = JWTOptions.Instance;
            //配置JWT认证
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(x =>
           {
               x.TokenValidationParameters = new()
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = jwtOpt.Issuer,
                   ValidAudience = jwtOpt.Audience,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.SecretKey))
               };
           });
            return services;
        }
    }
}
