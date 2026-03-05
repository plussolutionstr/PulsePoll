namespace PulsePoll.Mobile.ApiModels;

public record NotificationApiDto(
    int Id,
    string Title,
    string Body,
    string? Type,
    bool IsRead,
    DateTime CreatedAt);
