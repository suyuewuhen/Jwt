# JWT 工具库

一个轻量、可扩展的.NET JWT认证工具库，支持多级配置降级，开箱即用。

## ✨ 功能特性
- **双令牌机制**：支持AccessToken+RefreshToken双令牌架构，AccessToken短周期无需查存储高性能，RefreshToken长周期支持续期
- **令牌安全加固**：RefreshToken高熵随机生成、一次性使用、支持设备绑定、单设备登录控制
- **多级配置降级**：数据库配置优先，失败自动降级到嵌入式配置
- **多存储适配**：刷新令牌支持内存缓存（开发）、Redis（生产），可扩展其他分布式缓存
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
NuGet\Install-Package JwtLibrary -Version 1.0.0
```

## ⚙️ 配置说明
### 1. 配置文件配置
在项目的 `appsettings.json` 中添加以下配置（嵌入式配置模式下需将appsettings.json设置为**嵌入的资源**）：
```json
{
  "JWT": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SecretKey": "your-32bit-secret-key-at-least-16-characters",
    "ExpireSeconds": 3600, // AccessToken过期时间（秒）
    "RefreshTokenExpireDays": 7, // RefreshToken过期时间（天，优先级低于秒级配置）
    "RefreshTokenExpireSeconds": 0, // RefreshToken过期时间（秒，优先级高于天级配置，大于0时生效）
    "SingleDeviceLogin": false // 是否启用单设备登录（默认false，允许多设备同时在线）
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-sqlserver-connection-string",
    "Redis": "127.0.0.1:6379,password=your-redis-password" // Redis连接字符串（使用Redis存储刷新令牌时需要）
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

// 基础模式+双令牌+Redis存储
builder.Services.AddJwtToolkit(enableRefreshTokens: true)
                .UseRedisRefreshTokenStore(builder.Configuration.GetConnectionString("Redis"));

// 数据库模式+双令牌+Redis存储
// builder.Services.AddJwtToolkitWithDatabase(
//                     options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
//                     enableRefreshTokens: true)
//                 .UseRedisRefreshTokenStore(builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddJwtAuthentication();
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

### IJwtParser（令牌解析）
```csharp
// 验证并解析令牌，成功返回ClaimsPrincipal，失败返回null
ClaimsPrincipal? ValidateToken(string token);

// 从令牌中获取用户ID，失败返回Guid.Empty
Guid GetUserId(string token);

// 从令牌中获取用户名，失败返回空字符串
string GetUserName(string token);

// 从令牌中获取角色列表，失败返回空集合
List<string> GetRoles(string token);

// 不安全解析（不验证签名），返回所有声明
IEnumerable<Claim> ParseClaimsUnsafe(string token);
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
3. **过期时间**：建议AccessToken过期时间设置为15分钟到2小时，RefreshToken建议设置为7-30天
4. **生产环境存储**：生产环境必须使用Redis存储刷新令牌，内存缓存仅适合开发环境，服务重启令牌会丢失
5. **权限校验**：JWT只负责身份认证，接口权限请单独做校验
6. **设备绑定**：建议传入设备ID（如UserAgent+IP哈希），防止刷新令牌被盗用

## 📄 开源协议
MIT License
