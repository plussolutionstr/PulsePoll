using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record NotificationDto(
    int Id,
    string Title,
    string Body,
    string? Type,
    bool IsRead,
    DateTime CreatedAt);
