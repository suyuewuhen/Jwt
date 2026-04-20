using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Jwt
{
    internal class test
    {
        public static void Main(string[] args)
        {
            var token = new test().GetToken();
            Console.WriteLine(token);
            var userName = JWTHelper.GetUserName(token);
            Console.WriteLine(userName);
            var userId = JWTHelper.GetUserId(token);
            Console.WriteLine(userId);
            var roles = JWTHelper.GetRoles(token);
            Console.WriteLine(string.Join(",", roles));

        }

        public string GetToken()
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                //new System.Security.Claims.Claim(ClaimTypes.Name, "test"),
                //new System.Security.Claims.Claim(ClaimTypes.Role, "admin"),
                //new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.Name, "yiyiy"));
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, "admin1"));
            ITokenService tokenService = new TokenServer();
            return tokenService.BuilderToken(claims);
        }

        
    }
}
