using Corp.IdentityMgmt.Application.Interfaces;
using System;
using System.Security.Cryptography;

namespace Corp.IdentityMgmt.Infrastructure.Security
{
    public class PassowordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int SaltLength = 32;
        private const int Iterations = 100_000;

        public PasswordHashResult Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            var saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Use the static Pbkdf2 method to avoid obsolete constructors (SYSLIB0060).
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: SaltLength
            );

            return new PasswordHashResult(
                Hash: Convert.ToBase64String(hashBytes),
                Salt: Convert.ToBase64String(saltBytes)
            );
        }

        public bool Verify(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                return false;

            byte[] saltBytes;
            byte[] expectedHashBytes;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
                expectedHashBytes = Convert.FromBase64String(hash);
            }
            catch (FormatException)
            {
                // Provided hash or salt is not valid Base64.
                return false;
            }

            var derivedBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: expectedHashBytes.Length
            );

            return CryptographicOperations.FixedTimeEquals(derivedBytes, expectedHashBytes);
        }
    }
}
