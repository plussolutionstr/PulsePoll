using FluentValidation;
using PulsePoll.Application.Exceptions;
using Serilog;

namespace PulsePoll.Admin.Services;

public static class ErrorHandler
{
    private const string GenericUserMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";

    /// <summary>
    /// Form kayıt hataları için.
    /// BusinessException / ValidationException / NotFoundException -> form'a mesaj döner.
    /// Beklenmeyen hatalar -> kullanıcıya genel mesaj toast gösterilir.
    /// Tüm exception detayları loglanır.
    /// </summary>
    public static string? HandleFormSave(Exception ex, IAppToastService toast) => ex switch
    {
        BusinessException b => HandleKnownException(b),
        ValidationException v => HandleKnownException(v, v.Errors.FirstOrDefault()?.ErrorMessage ?? v.Message),
        NotFoundException n => HandleKnownException(n),
        _ => HandleUnexpectedForForm(ex, toast)
    };

    /// <summary>
    /// Genel operasyonlar (silme vb.) için -> kullanıcıya mesaj toast olarak gösterilir.
    /// Tüm exception detayları loglanır.
    /// </summary>
    public static void HandleOperation(Exception ex, IAppToastService toast, string? fallbackMessage = null)
    {
        var message = ex switch
        {
            BusinessException b => HandleKnownException(b),
            ValidationException v => HandleKnownException(v, v.Errors.FirstOrDefault()?.ErrorMessage ?? v.Message),
            NotFoundException n => HandleKnownException(n),
            _ => HandleUnexpected(ex, fallbackMessage)
        };

        toast.Error(message);
    }

    private static string HandleKnownException(Exception ex, string? message = null)
    {
        Log.Error(ex, "Handled exception");
        return message ?? ex.Message;
    }

    private static string HandleUnexpected(Exception ex, string? fallbackMessage = null)
    {
        Log.Error(ex, "Unexpected exception");
        return fallbackMessage ?? GenericUserMessage;
    }

    private static string? HandleUnexpectedForForm(Exception ex, IAppToastService toast)
    {
        toast.Error(HandleUnexpected(ex));
        return null;
    }
}
