using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jwt
{
    public class JWTOptions
    {
        public string Issuer { get; set; } 
        public string Audience { get; set; } 
        public string SecretKey { get; set; } 
        public int ExpireSeconds { get; set; } 


        public static JWTOptions Instance { get; } = LoadConfig();

        /// <summary>
        /// 优先从数据库加载配置，如果失败则从嵌入的资源中加载配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static JWTOptions LoadConfig()
        {
            try
            {
                // 1. 先尝试从数据库加载
                var dbConfig = LoadFromSqlConfig();
                if (dbConfig != null && !string.IsNullOrEmpty(dbConfig.SecretKey))
                {
                    return dbConfig;
                }
            }
            catch
            {
                // 数据库异常 → 降级用内嵌配置
            }

            // 2. 数据库没有/失败 → 使用内嵌配置
            return LoadFromEmbeddedConfig();
        }




        /// <summary>
        /// 从数据库中加载配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static JWTOptions LoadFromSqlConfig()
        {
            var provider = new DbJwtConfigProvider();
            return provider.GetJwtTokenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        }

        /// <summary>
        /// 从嵌入的资源中加载配置
        /// </summary>
        /// <returns></returns>
        private static JWTOptions LoadFromEmbeddedConfig()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.appsettings.json";
            var configuration = new ConfigurationBuilder();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                configuration.AddJsonStream(stream);
            }
            var config = configuration.Build();

            //从配置文件中获取JWT配置，如果没有则使用默认值
            //var jwtOpt = new JWTOptions();
            //config.GetSection("JWT").Bind(jwtOpt);
            //上面是新建->覆盖，建议下面是获取->覆盖（没有则新建），避免不必要的对象创建
            var jwtOpt = config.GetSection("JWT").Get<JWTOptions>() ?? new JWTOptions();
            if (string.IsNullOrEmpty(jwtOpt.SecretKey))
            {
                jwtOpt.SecretKey = "nideyiziyijuyouruzailhuaxingshanmh";
                jwtOpt.Issuer = "DefaultIssuer";
                jwtOpt.Audience = "DefaultAudience";
                jwtOpt.ExpireSeconds = 86400;
            }

            return jwtOpt;
        }



    }


    }
