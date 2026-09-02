using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace eAgenda.WebApi.Compartilhado.Identity;

public sealed record AcessTokenResponse(string AcessToken, DateTime DataExpiracaoEmUtc);
public sealed class JwtProvider(JwtOptions options)
{
    public AcessTokenResponse CriarToken(IdentityUser<Guid> user)
    {
        DateTime dataCriacao = DateTime.UtcNow;

        DateTime dataExpiracao = dataCriacao.AddMinutes(options.AccessTokenMinutes);

        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        ];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));

        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: dataCriacao,
            expires: dataExpiracao,
            signingCredentials: credentials
        );

        string acessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AcessTokenResponse(acessToken, dataExpiracao);
    }
}
