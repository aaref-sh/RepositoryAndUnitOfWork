using System.IdentityModel.Tokens.Jwt;

namespace Helper.Helpers;

public class JWTHelper
{
    public static long? GetUserIdFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token) ?? throw new ArgumentException("Invalid JWT token");
        _ = long.TryParse(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value, out var res);
        return res;
    }
}
