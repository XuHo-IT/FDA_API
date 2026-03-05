using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

class PasswordHashGenerator
{
    static void Main(string[] args)
    {
        string password = args.Length > 0 ? args[0] : "Admin@123";
        string hash = HashPassword(password);

        Console.WriteLine("===========================================");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash:     {hash}");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("Use this in SQL:");
        Console.WriteLine($"PasswordHash = '{hash}'");
        Console.WriteLine();
    }

    static string HashPassword(string password)
    {
        const int SaltSize = 16;
        const int HashSize = 32;
        const int IterationCount = 10000;

        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: IterationCount,
            numBytesRequested: HashSize
        );

        var outputBytes = new byte[1 + SaltSize + HashSize];
        outputBytes[0] = 0x01;
        Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
        Buffer.BlockCopy(hash, 0, outputBytes, 1 + SaltSize, HashSize);

        return Convert.ToBase64String(outputBytes);
    }
}






