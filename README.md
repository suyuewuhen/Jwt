# JWT 工具库

一个轻量、可扩展的.NET JWT认证工具库，支持多级配置降级，开箱即用。

## ✨ 功能特性
- **双令牌机制**：支持AccessToken+RefreshToken双令牌架构，AccessToken短周期无需查存储高性能，RefreshToken长周期支持续期
- **滑动过期**：支持RefreshToken滑动过期，用户活跃时自动延长有效期，无需频繁登录
- **安全加固**：高熵随机RefreshToken、一次性使用、设备绑定、单设备登录控制、刷新失败限流防暴力破解
- **令牌吊销**：支持单个令牌吊销、全设备批量吊销、AccessToken黑名单主动失效
- **多级配置降级**：数据库配置优先，失败自动降级到嵌入式配置，支持配置热更新无需重启
- **安全审计**：内置审计日志接口，所有令牌生成/刷新/吊销操作可追踪，支持自定义审计实现
- **自动无感续期**：内置中间件自动检测AccessToken即将过期，在响应头返回新令牌，前端无需主动调用刷新
- **多存储适配**：刷新令牌/黑名单统一使用IDistributedCache，支持内存缓存（开发）、Redis（生产），可扩展
- **多数据库兼容**：天然支持SqlServer/MySQL/PostgreSQL/SQLite等所有EF Core兼容数据库
- **ASP.NET Core 友好**：提供一键集成扩展方法，自动配置认证中间件
- **可选依赖**：无数据库场景可使用纯嵌入式配置，不依赖EF Core
- **配置校验**：启动时自动校验配置合法性，避免运行时隐藏错误
- **接口化设计**：完全基于依赖注入，单元测试友好

## 📦 安装
### 方式1：项目引用
直接将本项目添加到你的解决方案中引用即可。

### 方式2：NuGet（后续发布）
```bash
NuGet\Install-Package JwtLibrary -Version 1.0.2
```

## ⚙️ 配置说明
### 1. 配置文件配置
在项目的 `appsettings.json` 中添加以下配置（嵌入式配置模式下需将appsettings.json设置为**嵌入的资源**）：
```json
{
  "JWT": {
    // 基础配置（必填）
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SecretKey": "your-32bit-secret-key-at-least-16-characters",
    "ExpireSeconds": 3600, // AccessToken过期时间（秒）
    // 双令牌配置（可选，启用enableRefreshTokens时需要）
    "RefreshTokenExpireDays": 7, // RefreshToken过期时间（天，优先级低于秒级配置）
    "RefreshTokenExpireSeconds": 0, // RefreshToken过期时间（秒，优先级高于天级配置）
    "SingleDeviceLogin": false, // 是否启用单设备登录
    // 滑动过期配置（可选）
    "EnableSlidingExpiration": true, // 是否启用滑动过期
    "SlidingExpirationThreshold": 86400, // 剩余有效期小于多少秒时自动续期
    "SlidingExtendSeconds": 0, // 续期后有效期（秒），0表示续期到初始时长
    // 安全限流配置（可选）
    "EnableRefreshRateLimit": true, // 是否启用刷新失败限流
    "RefreshRateLimitCount": 5, // 最大失败次数
    "RefreshRateLockSeconds": 600, // 锁定时长（秒）
    // 黑名单配置（可选）
    "EnableAccessTokenBlacklist": false, // 是否启用AccessToken黑名单
    "BlacklistPrefix": "jwt:blacklist:", // 黑名单缓存键前缀
    // 热更新配置（可选）
    "EnableHotReload": false, // 是否启用配置热更新
    "HotReloadIntervalSeconds": 300 // 配置检查间隔（秒）
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-sqlserver-connection-string",
    "Redis": "127.0.0.1:6379,password=your-redis-password"
  }
}
```

### 2. 数据库配置
在数据库的 `SystemConfigs` 表中添加以下配置（数据库模式下使用）：
| ConfigKey | ConfigValue | 说明 |
|-----------|-------------|------|
| JwtSecretKey | your-secret-key | 签名密钥 |
| JwtIssuer | your-issuer | 令牌颁发者 |
| JwtAudience | your-audience | 令牌受众 |
| JwtExpireMinutes | 60 | AccessToken过期时间（分钟） |
| JwtRefreshExpireDays | 7 | RefreshToken过期时间（天） |
| JwtRefreshExpireSeconds | 0 | RefreshToken过期时间（秒，优先级更高） |
| JwtSingleDeviceLogin | false | 是否启用单设备登录 |
| JwtEnableSlidingExpiration | true | 是否启用滑动过期 |
| JwtSlidingExpirationThreshold | 86400 | 滑动过期阈值（秒） |
| JwtSlidingExtendSeconds | 0 | 滑动续期时长（秒，0=初始时长） |
| JwtEnableRefreshRateLimit | true | 是否启用刷新失败限流 |
| JwtRefreshRateLimitCount | 5 | 最大失败次数 |
| JwtRefreshRateLockSeconds | 600 | 锁定时长（秒） |
| JwtEnableAccessTokenBlacklist | false | 是否启用AccessToken黑名单 |
| JwtEnableHotReload | false | 是否启用配置热更新 |
| JwtHotReloadIntervalSeconds | 300 | 配置检查间隔（秒） |

## 🚀 快速开始
### 基础用法（无数据库依赖）
在 `Program.cs` 中添加服务注册：
```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加JWT工具库服务
builder.Services.AddJwtToolkit();
// 自动配置JWT认证中间件
builder.Services.AddJwtAuthentication();

builder.Services.AddControllers();
// ... 其他服务注册

var app = builder.Build();

// 启用认证中间件
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### 高级用法（数据库+嵌入式降级）
```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加带数据库支持的JWT服务
builder.Services.AddJwtToolkitWithDatabase(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// 自动配置JWT认证中间件
builder.Services.AddJwtAuthentication();

// ... 其余配置同上
```

### 双令牌+Redis配置（生产环境推荐）
```csharp
var builder = WebApplication.CreateBuilder(args);

// 基础模式+双令牌+Redis存储+所有高级特性
builder.Services.AddJwtToolkit(enableRefreshTokens: true)
                .UseRedisRefreshTokenStore(builder.Configuration.GetConnectionString("Redis"));

// 可选：注入自定义安全审计日志
// builder.Services.AddJwtAuditLogger<CustomJwtAuditLogger>();

// 数据库模式+双令牌+Redis存储
// builder.Services.AddJwtToolkitWithDatabase(
//                     options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
//                     enableRefreshTokens: true)
//                 .UseRedisRefreshTokenStore(builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddJwtAuthentication();

var app = builder.Build();

app.UseJwtAutoRenewal(); // 启用自动无感续期中间件
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

#### 多数据库适配
本库天然支持所有EF Core兼容的数据库，仅需更换数据库驱动和配置即可：

##### MySQL
```bash
# 先安装MySQL驱动
Install-Package Pomelo.EntityFrameworkCore.MySql
```
```csharp
builder.Services.AddJwtToolkitWithDatabase(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 23)));
});
```

##### PostgreSQL
```bash
# 先安装PostgreSQL驱动
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
```
```csharp
builder.Services.AddJwtToolkitWithDatabase(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
```

##### SQLite
```bash
# 先安装SQLite驱动
Install-Package Microsoft.EntityFrameworkCore.Sqlite
```
```csharp
builder.Services.AddJwtToolkitWithDatabase(options =>
{
    options.UseSqlite("Data Source=jwtconfig.db");
});
```

#### 自定义数据源扩展
如果需要对接非EF Core数据源（MongoDB、Redis、配置中心等），只需实现`IJwtConfigProvider`接口并替换默认注册即可：
```csharp
public class CustomJwtConfigProvider : IJwtConfigProvider
{
    public JwtOptions GetJwtConfig()
    {
        // 从自定义数据源加载配置逻辑
    }

    public Task<JwtOptions> GetJwtConfigAsync(CancellationToken cancellationToken = default)
    {
        // 异步加载逻辑
    }
}

// 注册自定义配置提供程序
builder.Services.AddSingleton<IJwtConfigProvider, CustomJwtConfigProvider>();
```

## 🔧 高级特性
### 安全审计日志
实现`IJwtAuditLogger`接口并注册即可自动记录所有令牌操作，无需改动业务代码：
```csharp
public class CustomJwtAuditLogger : IJwtAuditLogger
{
    private readonly ILogger<CustomJwtAuditLogger> _logger;

    public CustomJwtAuditLogger(ILogger<CustomJwtAuditLogger> logger) => _logger = logger;

    public Task LogTokenGeneratedAsync(Guid userId, string deviceId, IEnumerable<Claim> claims)
    {
        _logger.LogInformation("令牌生成 UserId={UserId} DeviceId={DeviceId}", userId, deviceId);
        return Task.CompletedTask;
    }

    public Task LogTokenRefreshedAsync(Guid userId, string deviceId, string oldRefreshToken)
    {
        _logger.LogInformation("令牌刷新 UserId={UserId}", userId);
        return Task.CompletedTask;
    }

    public Task LogTokenRevokedAsync(Guid userId, string refreshToken, string reason = "")
    {
        _logger.LogWarning("令牌吊销 UserId={UserId} Reason={Reason}", userId, reason);
        return Task.CompletedTask;
    }

    public Task LogAllUserTokensRevokedAsync(Guid userId, string reason = "")
    {
        _logger.LogWarning("全量令牌吊销 UserId={UserId} Reason={Reason}", userId, reason);
        return Task.CompletedTask;
    }

    public Task LogRefreshTokenFailedAsync(string refreshToken, string deviceId, string failReason)
    {
        _logger.LogWarning("刷新失败 DeviceId={DeviceId} Reason={Reason}", deviceId, failReason);
        return Task.CompletedTask;
    }
}

// 注册审计日志
builder.Services.AddJwtAuditLogger<CustomJwtAuditLogger>();
```

### AccessToken黑名单
启用后可主动吊销未过期的AccessToken，适用于账号封禁/异常退出等场景：
```csharp
private readonly IJwtParser _jwtParser;

[HttpPost("ban-user/{userId}")]
public async Task<IActionResult> BanUser(Guid userId)
{
    // 获取用户的当前令牌并加入黑名单
    var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
    await _jwtParser.AddToBlacklistAsync(token, jwtToken.ValidTo);
    
    // 吊销所有刷新令牌
    await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
    return Ok();
}
```

### 自动续期中间件
启用后客户端无需主动调用刷新接口，AccessToken剩余有效期少于5分钟时自动在响应头返回新令牌：
```csharp
// Program.cs 添加中间件
app.UseJwtAutoRenewal();
```
客户端收到响应头 `X-New-Access-Token` 和 `X-New-Refresh-Token` 后更新本地令牌即可，注意客户端需在每次请求的 `X-Refresh-Token` 头中传入RefreshToken。

### 滑动过期与防暴力破解
以下特性开箱即用，通过配置即可控制：
- **滑动过期**：用户活跃时RefreshToken自动续期，剩余有效期少于阈值时自动延长
- **刷新限流**：默认启用，同一RefreshToken刷新失败5次后锁定10分钟
- **设备绑定**：刷新令牌绑定设备ID，异地刷新自动拒绝

## 📖 API 参考
### ITokenService（令牌生成/管理）
```csharp
// 生成单个JWT访问令牌
string BuildToken(IEnumerable<Claim> claims);

// 生成双令牌（AccessToken + RefreshToken），deviceId可选用于设备绑定
Task<(string accessToken, string refreshToken)> BuildTokensAsync(IEnumerable<Claim> claims, string deviceId = "");

// 刷新双令牌，刷新成功返回新的双令牌，失败返回null
Task<(string accessToken, string refreshToken)?> RefreshTokensAsync(string refreshToken, string deviceId = "");

// 吊销指定刷新令牌（退出登录）
Task RevokeRefreshTokenAsync(string refreshToken);

// 吊销用户所有刷新令牌（密码修改/安全风控场景）
Task RevokeAllUserRefreshTokensAsync(Guid userId);
```

### IJwtParser（令牌解析与撤销）
```csharp
// 验证并解析令牌，成功返回ClaimsPrincipal，失败返回null
ClaimsPrincipal? ValidateToken(string token);

// 从令牌中获取用户ID，失败返回Guid.Empty
Guid GetUserId(string token);

// 从令牌中获取用户名，失败返回空字符串
string GetUserName(string token);

// 从令牌中获取角色列表，失败返回空集合
List<string> GetRoles(string token);

// 获取手机号，失败返回空字符串
string GetUserPhone(string token);

// 获取邮箱，失败返回空字符串
string GetUserEmail(string token);

// 获取租户ID，失败返回Guid.Empty
Guid GetTenantId(string token);

// 获取自定义Claim值，失败返回空字符串
string GetClaimValue(string token, string claimType);

// 不安全解析（不验证签名），返回所有声明
IEnumerable<Claim> ParseClaimsUnsafe(string token);

// 检查AccessToken是否在黑名单中
Task<bool> IsTokenBlacklistedAsync(string token);

// 将AccessToken加入黑名单（主动吊销未过期令牌）
Task AddToBlacklistAsync(string token, DateTime expireTime);
```

## 使用示例
### 单令牌模式
```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IJwtParser _jwtParser;

    public AuthController(ITokenService tokenService, IJwtParser jwtParser)
    {
        _tokenService = tokenService;
        _jwtParser = jwtParser;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 验证用户名密码逻辑...
        Guid userId = Guid.NewGuid(); // 实际从数据库读取用户ID
        
        // 生成令牌
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, "user")
        };
        var token = _tokenService.BuildToken(claims);
        
        return Ok(new { Token = token });
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = _jwtParser.GetUserId(token);
        var userName = _jwtParser.GetUserName(token);
        var roles = _jwtParser.GetRoles(token);
        
        return Ok(new { UserId = userId, UserName = userName, Roles = roles });
    }
}
```

### 双令牌模式
```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IJwtParser _jwtParser;

    public AuthController(ITokenService tokenService, IJwtParser jwtParser)
    {
        _tokenService = tokenService;
        _jwtParser = jwtParser;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 验证用户名密码逻辑...
        Guid userId = Guid.NewGuid(); // 实际从数据库读取用户ID
        
        // 生成双令牌，传入设备ID用于设备绑定（可选）
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, "user")
        };
        var deviceId = Request.Headers["User-Agent"].ToString(); // 可自定义设备标识
        var (accessToken, refreshToken) = await _tokenService.BuildTokensAsync(claims, deviceId);
        
        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var deviceId = Request.Headers["User-Agent"].ToString();
        var tokens = await _tokenService.RefreshTokensAsync(request.RefreshToken, deviceId);
        if (tokens == null)
            return Unauthorized("刷新令牌无效或已过期");
            
        return Ok(new { AccessToken = tokens.Value.accessToken, RefreshToken = tokens.Value.refreshToken });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok("退出成功");
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _tokenService.RevokeAllUserRefreshTokensAsync(Guid.Parse(userId));
        return Ok("所有设备已退出");
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = _jwtParser.GetUserId(token);
        var userName = _jwtParser.GetUserName(token);
        var roles = _jwtParser.GetRoles(token);
        
        return Ok(new { UserId = userId, UserName = userName, Roles = roles });
    }
}

// 请求实体定义
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
```

## ⚠️ 注意事项
1. **密钥安全**：生产环境请使用至少16位以上的复杂密钥，禁止硬编码在代码中
2. **HTTPS**：生产环境必须使用HTTPS传输令牌，避免泄露
3. **过期时间**：建议AccessToken设置为15分钟-2小时，RefreshToken设置为7-30天，避免过长造成安全风险
4. **生产环境存储**：必须使用Redis存储刷新令牌，内存缓存仅适合开发环境，重启令牌丢失且不支持多实例
5. **权限校验**：JWT只负责身份认证，接口权限请单独做校验
6. **设备绑定**：建议传入设备ID（如UserAgent+IP哈希），防止刷新令牌被盗用
7. **自动续期**：启用`UseJwtAutoRenewal()`后，回调URL需处理网关响应头，或前端统一封装请求拦截器
8. **审计日志**：建议在生产环境实现`IJwtAuditLogger`并接入日志中心，用于安全溯源

## 📄 开源协议
MIT License
