using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Kogase.Engine.Services
{
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            // Generate a 128-bit salt using a sequence of random non-zero values
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32));

            // Format: {iterations}.{salt}.{hash}
            return $"100000.{Convert.ToBase64String(salt)}.{hashed}";
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            // Extract the parts from the hashed password
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            // Get iteration count and salt
            if (!int.TryParse(parts[0], out int iterations))
            {
                return false;
            }
            
            byte[] salt = Convert.FromBase64String(parts[1]);
            
            // Hash the incoming password
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: 32));

            // Compare the hashed values
            return parts[2] == hashed;
        }
    }
} 