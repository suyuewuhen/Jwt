using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Jwt
{
    public interface ITokenService
    {
        string BuilderToken(IEnumerable<Claim> claims);
    }
}
