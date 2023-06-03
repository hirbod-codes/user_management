namespace user_management.Utilities;

public interface IStringHelper
{
    public string? HashWithoutSalt(string input, string method = "SHA512");
    public string Hash(string str);
    public bool DoesHashMatch(string hashedStr, string rawStr);
    public string GenerateRandomString(int size);
}