namespace PulsePoll.Application.Interfaces;

public interface ISesCalculator
{
    Task<int?> CalculateSesIdAsync(
        int? professionId,
        int? educationLevelId,
        bool isRetired,
        bool isHeadOfFamily,
        int? headOfFamilyProfessionId = null,
        int? headOfFamilyEducationLevelId = null,
        bool isHeadOfFamilyRetired = false);
}
