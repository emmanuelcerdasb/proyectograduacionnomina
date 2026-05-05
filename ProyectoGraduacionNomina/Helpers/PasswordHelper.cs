using System;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoGraduacionNomina.Helpers
{
    public static class PasswordHelper
    {
        // Hashea una contraseña usando SHA256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Compara una contraseña ingresada con la contraseña almacenada
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashOfInput = HashPassword(enteredPassword);
            return hashOfInput.Equals(storedHash);
        }
    }
}
