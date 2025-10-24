using Duotify.Membership.Api.Dtos;
using System.Net;

namespace Duotify.Membership.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception.Message switch
        {
            "NATIONAL_ID_ALREADY_EXISTS" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "NATIONAL_ID_ALREADY_EXISTS",
                    Message = "此身分證字號已被註冊"
                }
            },
            "INVALID_CREDENTIALS" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "帳號或密碼不正確"
                }
            },
            "CODE_EXPIRED" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "CODE_EXPIRED",
                    Message = "驗證碼已過期，請重新發送"
                }
            },
            "INVALID_CODE" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "INVALID_CODE",
                    Message = "驗證碼不正確"
                }
            },
            "TOO_MANY_ATTEMPTS" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "TOO_MANY_ATTEMPTS",
                    Message = "嘗試次數過多，請重新發送驗證碼"
                }
            },
            "ALREADY_VERIFIED" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "ALREADY_VERIFIED",
                    Message = "此帳號已驗證"
                }
            },
            "MEMBER_NOT_FOUND" => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "MEMBER_NOT_FOUND",
                    Message = "帳號不存在"
                }
            },
            _ => new
            {
                success = false,
                error = new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "發生系統錯誤"
                }
            }
        };

        context.Response.StatusCode = exception.Message switch
        {
            "NATIONAL_ID_ALREADY_EXISTS" => (int)HttpStatusCode.Conflict,
            "INVALID_CREDENTIALS" => (int)HttpStatusCode.Unauthorized,
            "CODE_EXPIRED" or "INVALID_CODE" or "TOO_MANY_ATTEMPTS" => (int)HttpStatusCode.BadRequest,
            "MEMBER_NOT_FOUND" => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
