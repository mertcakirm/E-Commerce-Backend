using System.Net;
using System.Text.Json.Serialization;

namespace eCommerce.Application;

public class ServiceResult<T>
{
    public T? Data { get; set; }
    public List<string>? ErrorMessage { get; set; }
    
    [JsonIgnore] public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
    [JsonIgnore] public bool IsFail => !IsSuccess;
    [JsonIgnore] public HttpStatusCode Status { get; set; }
    [JsonIgnore] public string? UrlAsCreated { get; set; }

    // SUCCESS
    public static ServiceResult<T> Success(T data, string? message = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new ServiceResult<T>()
        {
            Data = data,
            Status = status,
            ErrorMessage = message != null ? new List<string> { message } : null
        };
    }

    public static ServiceResult<T> SuccessAsCreated(T data, string url, string? message = null)
    {
        return new ServiceResult<T>()
        {
            Data = data,
            Status = HttpStatusCode.Created,
            UrlAsCreated = url,
            ErrorMessage = message != null ? new List<string> { message } : null
        };
    }

    // FAIL
    public static ServiceResult<T> Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        return new ServiceResult<T>()
        {
            ErrorMessage = errorMessage,
            Status = status
        };
    }

    public static ServiceResult<T> Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        return new ServiceResult<T>()
        {
            ErrorMessage = new List<string> { errorMessage },
            Status = status
        };
    }
}

// NOCONTENT veya NO RESPONSE DATA için
public class ServiceResult
{
    public List<string>? ErrorMessage { get; set; }

    [JsonIgnore] public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
    [JsonIgnore] public bool IsFail => !IsSuccess;
    [JsonIgnore] public HttpStatusCode Status { get; set; }

    // SUCCESS
    public static ServiceResult Success(string? message = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new ServiceResult()
        {
            Status = status,
            ErrorMessage = message != null ? new List<string> { message } : null
        };
    }

    public static ServiceResult NoContent(string? message = null)
    {
        return new ServiceResult()
        {
            Status = HttpStatusCode.NoContent,
            ErrorMessage = message != null ? new List<string> { message } : null
        };
    }

    // FAIL
    public static ServiceResult Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        return new ServiceResult()
        {
            ErrorMessage = errorMessage,
            Status = status
        };
    }

    public static ServiceResult Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        return new ServiceResult()
        {
            ErrorMessage = new List<string> { errorMessage },
            Status = status
        };
    }
}