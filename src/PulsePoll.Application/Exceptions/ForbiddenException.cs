namespace PulsePoll.Application.Exceptions;

public class ForbiddenException(string message = "Bu işlem için yetkiniz yok.")
    : Exception(message);
