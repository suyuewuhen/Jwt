using Jwt.Abstractions;
using Jwt.Implementations;
using Jwt.Implementations.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Jwt.Extensions;

/// <summary>
/// JWT服务注入扩展
/// </summary>
public static class JwtServiceCollectionExtensions
{
    /// <summary>
    /// 添加JWT工具库服务（仅嵌入式配置）
    /// </summary>
    public static IServiceCollection AddJwtToolkit(this IServiceCollection services)
    {
        // 注册配置提供程序
        services.AddSingleton<IJwtConfigProvider, EmbeddedJwtConfigProvider>();
        
        // 注册JWT配置
        services.AddSingleton(sp =>
        {
            var provider = sp.GetRequiredService<IJwtConfigProvider>();
            return provider.GetJwtConfig();
        });

        // 注册核心服务
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IJwtParser, JwtTokenService>();

        return services;
    }

    /// <summary>
    /// 添加JWT工具库服务（数据库+嵌入式降级配置）
    /// </summary>
    public static IServiceCollection AddJwtToolkitWithDatabase(this IServiceCollection services, 
        Action<DbContextOptionsBuilder> dbContextOptionsAction)
    {
        // 注册数据库上下文
        services.AddDbContext<JwtDbContext>(dbContextOptionsAction);
        
        // 注册配置提供程序（数据库优先，嵌入式降级）
        services.AddSingleton<IJwtConfigProvider>(sp =>
        {
            var dbProvider = ActivatorUtilities.CreateInstance<DbJwtConfigProvider>(sp);
            var embeddedProvider = new EmbeddedJwtConfigProvider();
            return new CompositeJwtConfigProvider(new IJwtConfigProvider[] { dbProvider, embeddedProvider });
        });
        
        // 注册JWT配置
        services.AddSingleton(sp =>
        {
            var provider = sp.GetRequiredService<IJwtConfigProvider>();
            return provider.GetJwtConfig();
        });

        // 注册核心服务
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IJwtParser, JwtTokenService>();

        return services;
    }

    /// <summary>
    /// 添加JWT身份验证配置
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var jwtOptions = serviceProvider.GetRequiredService<JwtOptions>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
