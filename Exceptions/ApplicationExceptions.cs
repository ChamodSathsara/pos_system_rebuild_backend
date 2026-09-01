using Microsoft.AspNetCore.Http;

namespace PosApi.Exceptions;

/// <summary>
/// Base class for all handled application exceptions. Carries an HTTP status code so the
/// global exception handling middleware can translate it directly into a response.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public List<string>? Errors { get; }

    protected AppException(string message, int statusCode, List<string>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message, List<string>? errors = null)
        : base(message, StatusCodes.Status400BadRequest, errors)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, StatusCodes.Status409Conflict)
    {
    }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Invalid credentials.")
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}

public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "You do not have permission to perform this action.")
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

public class ValidationAppException : AppException
{
    public ValidationAppException(List<string> errors)
        : base("One or more validation errors occurred.", StatusCodes.Status422UnprocessableEntity, errors)
    {
    }
}
