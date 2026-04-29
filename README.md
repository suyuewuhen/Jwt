# JWT 工具库

一个轻量、可扩展的.NET JWT认证工具库，支持多级配置降级，开箱即用。

## ✨ 功能特性
- **令牌生成与验证**：支持标准JWT令牌生成、签名验证、过期校验
- **多级配置降级**：数据库配置优先，失败自动降级到嵌入式配置
- **ASP.NET Core 友好**：提供一键集成扩展方法，自动配置认证中间件
- **可选依赖**：无数据库场景可使用纯嵌入式配置，不依赖EF Core
- **配置校验**：启动时自动校验配置合法性，避免运行时隐藏错误
- **接口化设计**：完全基于依赖注入，单元测试友好

## 📦 安装
### 方式1：项目引用
直接将本项目添加到你的解决方案中引用即可。

### 方式2：NuGet（后续发布）
```bash
Install-Package CShaper.Jwt
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
    "ExpireSeconds": 86400
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-sqlserver-connection-string"
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
| JwtExpireMinutes | 1440 | 令牌过期时间（分钟） |

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

## 📖 API 参考
### ITokenService（令牌生成）
```csharp
// 生成JWT令牌
string BuildToken(IEnumerable<Claim> claims);
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

## ⚠️ 注意事项
1. **密钥安全**：生产环境请使用至少16位以上的复杂密钥，禁止硬编码在代码中
2. **HTTPS**：生产环境必须使用HTTPS传输令牌，避免泄露
3. **过期时间**：建议将过期时间设置为1-24小时，长时间令牌请配合刷新令牌使用
4. **权限校验**：JWT只负责身份认证，接口权限请单独做校验

## 📄 开源协议
MIT License
