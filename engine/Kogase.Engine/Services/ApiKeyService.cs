using System;
using System.Security.Cryptography;

namespace Kogase.Engine.Services
{
    public class ApiKeyService
    {
        public string GenerateApiKey()
        {
            // Generate a secure random API key
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            
            // Convert to base64 string for easier storage/transmission
            string apiKey = Convert.ToBase64String(bytes);
            
            // Replace characters that might cause issues in URLs
            apiKey = apiKey.Replace("/", "_").Replace("+", "-").Replace("=", "");
            
            return apiKey;
        }
        
        // Validates the API key format
        public bool IsValidApiKeyFormat(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }
            
            // API key should be 43 characters long (32 bytes in base64 without padding)
            if (apiKey.Length < 40)
            {
                return false;
            }
            
            // Should only contain alphanumeric characters, hyphens, and underscores
            foreach (char c in apiKey)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return false;
                }
            }
            
            return true;
        }
    }
} 