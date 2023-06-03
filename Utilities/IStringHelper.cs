namespace user_management.Utilities;

public interface IStringHelper
{
    public string Hash(string str);
    public string GenerateRandomString(int size);
}