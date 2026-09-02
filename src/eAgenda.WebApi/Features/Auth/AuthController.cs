using eAgenda.WebApi.Compartilhado.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace eAgenda.WebApi.Features.Auth;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager, JwtProvider jwtProvider
) : ControllerBase
{
    [HttpPost("registrar")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult> Registrar(RegistrarRequest req)
    {
        var user = new IdentityUser<Guid>()
        {
            Id = Guid.CreateVersion7(),
            UserName = req.Email.Trim(),
            Email = req.Email.Trim()
        };

        var resultado = await userManager.CreateAsync(user, req.Senha);

        if (!resultado.Succeeded)
        {
            foreach (IdentityError erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);

            return ValidationProblem(ModelState);
        }

        return Created(string.Empty, new UsuarioResponse(user.Id, user.Email));
    }

    [HttpPost("entrar")]
    public async Task<ActionResult<AcessTokenResponse>> Entrar(EntrarRequest req)
    {
        var usuario = await userManager.FindByEmailAsync(req.Email.Trim());

        if (usuario is null)
            return Unauthorized();

        var resultado = await signInManager.CheckPasswordSignInAsync(usuario, req.Senha, true);

        if (!resultado.Succeeded)
            return Unauthorized();

        return Ok(jwtProvider.CriarToken(usuario));
    }
}
