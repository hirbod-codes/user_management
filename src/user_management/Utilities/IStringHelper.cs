namespace user_management.Utilities;

public interface IStringHelper
{
    public string Base64Encode(string plainText);
    public string Base64Decode(string base64EncodedData);
    public string? HashWithoutSalt(string input, string method = "SHA512");
    public string Hash(string str);
    public bool DoesHashMatch(string hashedStr, string rawStr);
    public string GenerateRandomString(int size);
}