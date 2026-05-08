using System;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoGraduacionNomina.Helpers
{
    public static class PasswordHelper
    {
        private const int Iterations = 100_000;
        private const int SaltSize   = 32;
        private const int HashSize    = 32;

        // Devuelve hash PBKDF2 en formato: pbkdf2$<iter>$<salt_b64>$<hash_b64>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            byte[] hash = DeriveKey(password, salt, Iterations, HashSize);

            return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        // Soporta hashes PBKDF2 (nuevos) y SHA256 sin salt (legados para migración automática)
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            if (storedHash.StartsWith("pbkdf2$"))
                return VerifyPbkdf2(password, storedHash);

            // Hash legado SHA256: 64 caracteres hexadecimales
            if (storedHash.Length == 64)
                return VerifySha256Legacy(password, storedHash);

            return false;
        }

        // Indica si el hash almacenado es formato legado (SHA256) para migración en login
        public static bool IsLegacyHash(string storedHash)
        {
            return !string.IsNullOrEmpty(storedHash)
                && !storedHash.StartsWith("pbkdf2$")
                && storedHash.Length == 64;
        }

        // ──────────────────────────── privados ────────────────────────────

        private static bool VerifyPbkdf2(string password, string storedHash)
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4)
                return false;

            if (!int.TryParse(parts[1], out int iterations))
                return false;

            byte[] salt     = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual   = DeriveKey(password, salt, iterations, expected.Length);

            return SlowEquals(expected, actual);
        }

        private static bool VerifySha256Legacy(string password, string storedHash)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString() == storedHash;
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int keyLength)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(keyLength);
        }

        // Comparación en tiempo constante para evitar timing attacks
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
