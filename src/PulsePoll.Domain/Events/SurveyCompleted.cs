namespace PulsePoll.Domain.Events;

public record SurveyCompleted(
    int SurveyId,
    int SubjectId,
    string WebhookPayload,
    DateTime CompletedAt);
