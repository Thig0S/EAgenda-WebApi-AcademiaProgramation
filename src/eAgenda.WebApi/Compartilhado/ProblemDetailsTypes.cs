namespace eAgenda.WebApi.Compartilhado;

public static class ProblemDetailsTypes
{
    private const string BaseDocumentationUrl = "https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Reference/Status";

    public const string BadRequest = $"{BaseDocumentationUrl}/400";
    public const string NotFound = $"{BaseDocumentationUrl}/404";
    public const string Conflict = $"{BaseDocumentationUrl}/409";
    public const string InternalServerError = $"{BaseDocumentationUrl}/500";

    public static string? ObterPorStatus(int? statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => BadRequest,
            StatusCodes.Status404NotFound => NotFound,
            StatusCodes.Status409Conflict => Conflict,
            StatusCodes.Status500InternalServerError => InternalServerError,
            _ => null,
        };
    }
}
