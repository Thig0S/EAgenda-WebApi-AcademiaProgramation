using eAgenda.Aplicacao.Modulos.ModuloCompromisso;
using eAgenda.WebApi.Compartilhado;
using Microsoft.AspNetCore.Mvc;

namespace eAgenda.WebApi.Features.Compromissos;

[ApiController]
[Route("api/compromissos")]
public sealed class CompromissoController(ServicoCompromisso servicoCompromisso) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<ListarCompromissosDto>> SelecionarTodos()
    {
        return Ok(servicoCompromisso.SelecionarTodos());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<DetalhesCompromissoDto> SelecionarPorId(Guid id)
    {
        var resultado = servicoCompromisso.SelecionarPorId(id);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(resultado.Value);
    }
}
