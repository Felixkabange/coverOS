using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoverOS.Application.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace CoverOS.Api.Authentication;
public sealed class SessionIssuer(IConfiguration configuration) : ISessionIssuer
{
    public SessionDto Issue(AccountDto account)
    {
        var key = configuration["Jwt:SigningKey"]!;
        var expires = DateTime.UtcNow.AddMinutes(30);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"] ?? "CoverOS",
            configuration["Jwt:Audience"] ?? "CoverOS.Client",
            [new Claim(JwtRegisteredClaimNames.Sub, account.UserId.ToString()),
             new Claim("organization_id", account.OrganizationId.ToString()),
             new Claim(ClaimTypes.Role, account.Role), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())],
            expires: expires, signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), expires, account);
    }
}
