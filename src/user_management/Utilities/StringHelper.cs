namespace user_management.Utilities;

using System.Security.Cryptography;
using System.Text;

public class StringHelper : IStringHelper
{
    public readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public string Base64Encode(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public string Base64Decode(string base64EncodedData)
    {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public string? HashWithoutSalt(string input, string method = "SHA512")
    {
        StringBuilder sb = new StringBuilder();

        byte[] hash;
        switch (method)
        {
            case "SHA256":
                using (HashAlgorithm algorithm = SHA256.Create())
                    hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                break;

            case "SHA512":
                using (HashAlgorithm algorithm = SHA512.Create())
                    hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                break;
            default:
                return null;
        }

        foreach (byte b in hash)
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    public string Hash(string str)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(str, salt, 100000, HashAlgorithmName.SHA512);
        byte[] hash = pbkdf2.GetBytes(20);

        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
    }

    public bool DoesHashMatch(string hashedStr, string rawStr)
    {
        byte[] hashBytes = Convert.FromBase64String(hashedStr);

        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(rawStr, salt, 100000, HashAlgorithmName.SHA512);
        byte[] hash = pbkdf2.GetBytes(20);

        for (int i = 0; i < 20; i++)
            if (hashBytes[i + 16] != hash[i])
                return false;

        return true;
    }

    public string GenerateRandomString(int size)
    {
        byte[] data = new byte[4 * size];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }
        StringBuilder result = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % chars.Length;

            result.Append(chars[idx]);
        }

        return result.ToString();
    }
}
