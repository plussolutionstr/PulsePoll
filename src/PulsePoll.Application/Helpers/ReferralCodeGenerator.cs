namespace PulsePoll.Application.Helpers;

public static class ReferralCodeGenerator
{
    private const string ValidChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // Excludes 0,1,I,O,L

    public static string Generate(int length = 8)
    {
        var random = new Random();
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = ValidChars[random.Next(ValidChars.Length)];
        return new string(chars);
    }
}
