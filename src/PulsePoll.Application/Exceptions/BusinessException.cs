namespace PulsePoll.Application.Exceptions;

public class BusinessException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
