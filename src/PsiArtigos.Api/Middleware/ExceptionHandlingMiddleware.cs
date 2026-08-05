using System.Net;
using System.Text.Json;
using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            UnauthorizedAppException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            DomainException => (HttpStatusCode.BadRequest, "Domain Error"),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Title}", title);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            title,
            status = (int)statusCode,
            detail = exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
