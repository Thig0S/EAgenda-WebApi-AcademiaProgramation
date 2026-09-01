using eAgenda.Aplicacao.Modulos.ModuloCompromisso;
using eAgenda.WebApi.Compartilhado;
using FluentResults;
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
    [HttpPost]
    [ProducesResponseType<DetalhesCompromissoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DetalhesCompromissoDto> Cadastrar(CadastrarCompromissoRequest req)
    {
        var dto = new CadastrarCompromissoDto(
            req.Assunto,
            req.DataOcorrencia,
            req.HoraInicio,
            req.HoraTermino,
            req.Tipo,
            req.Local,
            req.Link,
            req.ContatoId
        );
        var resultado = servicoCompromisso.Cadastrar(dto);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        Guid id = resultado.Value;

        var resultadoSelecao = servicoCompromisso.SelecionarPorId(id);

        if (resultadoSelecao.IsFailed)
            return this.ProblemDetails(resultadoSelecao);

        return CreatedAtAction(
            nameof(SelecionarPorId),
            new { id },
            resultadoSelecao.Value
        );
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Editar(Guid id, EditarCompromissoRequest req)
    {
        EditarCompromissoDto dto = new(
            id,
            req.Assunto,
            req.DataOcorrencia,
            req.HoraInicio,
            req.HoraTermino,
            req.Tipo,
            req.Local,
            req.Link,
            req.ContatoId
        );

        var resultadoEdicao = servicoCompromisso.Editar(dto);

        if (resultadoEdicao.IsFailed)
            return this.ProblemDetails(resultadoEdicao);

        return NoContent();
    }
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Excluir(Guid id)
    {
        var resultadoExclusao = servicoCompromisso.Excluir(id);

        if(resultadoExclusao.IsFailed)
            return this.ProblemDetails(resultadoExclusao);

        return NoContent();
    }
}