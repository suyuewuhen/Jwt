using System;
using System.Collections.Generic;
using System.Text;

namespace Jwt
{
    public interface IJwtConfigProvider
    {
        Task<JWTOptions> GetJwtTokenAsync();
    }
}
