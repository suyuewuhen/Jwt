[Jwt_README.md](https://github.com/user-attachments/files/26894294/Jwt_README.md)
# JWT 认证工具库

一个轻量级的 .NET JWT (JSON Web Token) 认证工具库，支持令牌的生成、解析和验证。

## 功能特性

- **令牌生成**: 快速生成 JWT 令牌
- **令牌解析**: 解析令牌中的用户信息
- **令牌验证**: 验证令牌合法性、过期时间和签名
- **多配置源**: 支持从数据库或 appsettings.json 加载配置
- **ASP.NET Core 集成**: 简洁的认证配置扩展方法

## 项目结构

| 文件 | 说明 |
|------|------|
| `JWTHelper.cs` | 核心工具类，提供解析和验证方法 |
| `TokenServer.cs` | 令牌生成服务 |
| `ITokenService.cs` | 令牌服务接口 |
| `JWTOptions.cs` | JWT 配置选项 |
| `AuthenticationExtensions.cs` | ASP.NET Core 认证扩展 |

## 快速开始

### 1. 安装

将项目引用到你的 .NET 解决方案中，或使用 NuGet 安装相关依赖：

```bash
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 2. 配置

在 `appsettings.json` 中添加 JWT 配置：

```json
{
  "JWT": {
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "SecretKey": "YourSecretKeyMinimum32Characters",
    "ExpireSeconds": 86400
  }
}
```

### 3. 生成令牌

```csharp
using System.Collections.Generic;
using System.Security.Claims;

// 创建用户声明
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    new Claim(ClaimTypes.Name, username),
    new Claim(ClaimTypes.Role, "admin")
};

// 生成令牌
ITokenService tokenService = new TokenServer();
string token = tokenService.BuilderToken(claims);
```

### 4. 解析令牌

```csharp
// 获取用户Id
Guid userId = JWTHelper.GetUserId(token);

// 获取用户名
string userName = JWTHelper.GetUserName(token);

// 获取用户角色
List<string> roles = JWTHelper.GetRoles(token);

// 获取所有声明
IEnumerable<Claim> claims = JWTHelper.ParseClaims(token);
```

### 5. 验证令牌

```csharp
// 验证令牌（验证合法性、过期、签名）
ClaimsPrincipal? user = JWTHelper.ValidateToken(token);

if (user != null)
{
    // 令牌有效
}
else
{
    // 令牌无效
}
```

### 6. ASP.NET Core 集成

在 `Program.cs` 中配置认证：

```csharp
builder.Services.AddAuthenticationConfig();
```

然后在需要授权的控制器或方法上使用 `[Authorize]` 属性。

## API 参考

### JWTHelper

| 方法 | 说明 |
|------|------|
| `BuilderToken(claims)` | 生成令牌 (使用 TokenServer) |
| `ParseClaims(token)` | 解析并返回所有声明 |
| `GetUserId(token)` | 获取用户 ID |
| `GetUserName(token)` | 获取用户名 |
| `GetRoles(token)` | 获取用户角色列表 |
| `ValidateToken(token)` | 验证令牌，返回 ClaimsPrincipal |

### JWTOptions

| 属性 | 说明 |
|------|------|
| `Issuer` | 签发者 |
| `Audience` | 受众 |
| `SecretKey` | 密钥（至少32字符） |
| `ExpireSeconds` | 过期时间（秒） |

## 配置优先级

1. 优先从数据库加载配置（需要实现 DbJwtConfigProvider）
2. 回退到 appsettings.json 配置
3. 使用内置默认值

## 示例

完整的令牌生成和验证示例：

```csharp
using System;
using System.Collections.Generic;
using System.Security.Claims;

// 1. 生成令牌
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.Name, "zhangsan"),
    new Claim(ClaimTypes.Role, "admin"),
    new Claim(ClaimTypes.Role, "user")
};

ITokenService tokenService = new TokenServer();
string token = tokenService.BuilderToken(claims);

Console.WriteLine($"Token: {token}");

// 2. 解析令牌
Console.WriteLine($"UserId: {JWTHelper.GetUserId(token)}");
Console.WriteLine($"UserName: {JWTHelper.GetUserName(token)}");
Console.WriteLine($"Roles: {string.Join(", ", JWTHelper.GetRoles(token))}");

// 3. 验证令牌
var user = JWTHelper.ValidateToken(token);
Console.WriteLine($"Valid: {user != null}");
```

## 依赖

- .NET 6.0+
- Microsoft.IdentityModel.Tokens
- System.IdentityModel.Tokens.Jwt
- Microsoft.AspNetCore.Authentication.JwtBearer

## 许可

MIT License
