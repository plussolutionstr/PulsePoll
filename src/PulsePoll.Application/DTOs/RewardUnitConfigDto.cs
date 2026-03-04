namespace PulsePoll.Application.DTOs;

public record RewardUnitConfigDto(
    string UnitCode,
    string UnitLabel,
    decimal TryMultiplier);
