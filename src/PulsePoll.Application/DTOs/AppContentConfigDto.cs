namespace PulsePoll.Application.DTOs;

public record AppContentConfigDto(
    string KvkkText,
    string ContactTitle,
    string ContactBody,
    string? ContactEmail,
    string? ContactPhone,
    string? ContactWhatsapp);
