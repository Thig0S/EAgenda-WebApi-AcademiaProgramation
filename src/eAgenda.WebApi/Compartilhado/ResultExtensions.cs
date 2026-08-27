using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eAgenda.WebApi.Compartilhado;

public static class ResultExtensions
{
    public static ActionResult ParaErroDaApi(this ControllerBase controller, ResultBase result)
    {
        if (result.HasError(e =>
            e.Message.Equals("Já existe um contato com este email.") ||
            e.Message.Equals("Já existe um contato com este telefone.")))
        {
            return controller.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito",
                detail: result.Errors.First().Message,
                type: "https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Reference/Status/409"
            );
        }
        //erros de validacao
        var modelState = new ModelStateDictionary();
        foreach (var erro in result.Errors)
        {
            var campo = erro.Metadata["Campo"];

            modelState.AddModelError(campo.ToString()!, erro.Message);
        }
        ValidationProblemDetails problemDetails = new(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "requisição invalida"
        };

        return controller.StatusCode(StatusCodes.Status400BadRequest, problemDetails);
    }
}
