using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
        /// 读取配置文件
        /// </summary>
        /// <returns></returns>
        private IConfiguration LoadAppConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
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
                ExpireSeconds = int.Parse(GetValue(configs, "JwtExpireMinutes", "86400"))
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
