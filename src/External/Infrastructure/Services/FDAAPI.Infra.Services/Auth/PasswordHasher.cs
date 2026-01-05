using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace FDAAPI.Infra.Services.Auth
{
    /// <summary>
    /// Password hashing service using PBKDF2 (industry standard)
    /// - Uses random salt per password
    /// - 10,000 iterations (OWASP recommendation)
    /// - SHA256 hash function
    /// - Timing-safe comparison to prevent timing attacks
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        // OWASP recommendations
        private const int SaltSize = 128 / 8; // 16 bytes
        private const int HashSize = 256 / 8; // 32 bytes
        private const int IterationCount = 10000; // PBKDF2 iterations

        /// <summary>
        /// Hash password with random salt using PBKDF2-SHA256
        /// Format: [version_byte][salt_16_bytes][hash_32_bytes] ? Base64
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Generate random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive hash from password using PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize
            );

            // Combine version + salt + hash into single byte array
            var outputBytes = new byte[1 + SaltSize + HashSize];
            outputBytes[0] = 0x01; // Version 1 (for future algorithm upgrades)
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, outputBytes, 1 + SaltSize, HashSize);

            // Return Base64-encoded string
            return Convert.ToBase64String(outputBytes);
        }

        /// <summary>
        /// Verify password against stored hash (timing-safe comparison)
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrEmpty(passwordHash))
            {
                throw new ArgumentNullException(nameof(passwordHash));
            }

            // Decode Base64 hash
            byte[] decodedHash;
            try
            {
                decodedHash = Convert.FromBase64String(passwordHash);
            }
            catch (FormatException)
            {
                return false; // Invalid Base64
            }

            // Validate hash format
            if (decodedHash.Length != 1 + SaltSize + HashSize)
            {
                return false; // Invalid hash length
            }

            // Check version (future-proofing)
            byte version = decodedHash[0];
            if (version != 0x01)
            {
                return false; // Unsupported version
            }

            // Extract salt from stored hash
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(decodedHash, 1, salt, 0, SaltSize);

            // Extract expected hash
            byte[] expectedHash = new byte[HashSize];
            Buffer.BlockCopy(decodedHash, 1 + SaltSize, expectedHash, 0, HashSize);

            // Compute hash of input password with same salt
            byte[] actualHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize
            );

            // Timing-safe comparison (prevents timing attacks)
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}






