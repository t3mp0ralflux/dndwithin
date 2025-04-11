using System.Security.Cryptography;
using System.Text;

namespace DNDWithin.Application.Services.Implementation;


public class PasswordHasher : IPasswordHasher
{
    private const int HashSize = 256 / 8;
    private const int Iterations = 100000;
    private const int SaltSize = 128 / 8;
    internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_".ToCharArray();

    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }

    public string CreateActivationToken()
    {
        byte[] data = new byte[4 * 8];
        using (RandomNumberGenerator crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new(8);
        for (int i = 0; i < 8; i++)
        {
            uint rnd = BitConverter.ToUInt32(data, i * 4);
            long idx = rnd % chars.Length;

            result.Append(chars[idx]);
        }

        return result.ToString();
    }
}