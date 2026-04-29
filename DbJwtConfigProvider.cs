using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jwt
{
    //从数据库中获取jwt配置
    public class DbJwtConfigProvider : IJwtConfigProvider
    {
        private TDbcontext dbcontext;

        public DbJwtConfigProvider()
        {
            /// <summary>
            /// 读取内嵌appsetting.json中的sql配置项并连接数据库
            /// </summary>
            /// <returns></returns>
            var config  = LoadAppConfiguration();
            var conn = config.GetConnectionString("DefaultConnection");
            var options = new DbContextOptionsBuilder<TDbcontext>()
                .UseSqlServer(conn)
                .Options;
            dbcontext = new TDbcontext(options);
        }

        /// <summary>
        /// 读取嵌入式配置文件
        /// </summary>
        /// <returns></returns>
        private IConfiguration LoadAppConfiguration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.appsettings.json";
            var configuration = new ConfigurationBuilder();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                configuration.AddJsonStream(stream);
            }
            return configuration.Build();
        }

        /// <summary>
        /// 同步获取JWT配置
        /// </summary>
        /// <returns></returns>
        public JWTOptions GetJwtToken()
        {
            var configs = dbcontext.Set<SystemConfig>()
               .Where(c => c.ConfigKey.StartsWith("Jwt"))
               .ToList();
            return new JWTOptions
            {
                SecretKey = GetValue(configs, "JwtSecretKey"),
                Issuer = GetValue(configs, "JwtIssuer"),
                Audience = GetValue(configs, "JwtAudience"),
                ExpireSeconds = int.Parse(GetValue(configs, "JwtExpireMinutes", "1440")) * 60
            };
        }

        public async Task<JWTOptions> GetJwtTokenAsync()
        {
            var configs = await dbcontext.Set<SystemConfig>()
               .Where(c => c.ConfigKey.StartsWith("Jwt"))
               .ToListAsync();
            return new JWTOptions
            {
                SecretKey = GetValue(configs, "JwtSecretKey"),
                Issuer = GetValue(configs, "JwtIssuer"),
                Audience = GetValue(configs, "JwtAudience"),
                ExpireSeconds = int.Parse(GetValue(configs, "JwtExpireMinutes", "1440")) * 60
            };
        }

        

        private string GetValue(List<SystemConfig> configs, string key, string defaultValue = "")
        {
            return configs.FirstOrDefault(c => c.ConfigKey == key)?.ConfigValue ?? defaultValue;
        }


        // 你的系统配置表（只要字段对应即可）
        public class SystemConfig
        {
            public string ConfigKey { get; set; } = string.Empty;
            public string ConfigValue { get; set; } = string.Empty;
        }
    }
}
