namespace API.Errors;

public record ApiException(int StatusCode, string Message, string? Details);
