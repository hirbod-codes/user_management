namespace user_management.Utilities;

using System.Security.Cryptography;
using System.Text;

public class StringHelper : IStringHelper
{
    public readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

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

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(str, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);

        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
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
