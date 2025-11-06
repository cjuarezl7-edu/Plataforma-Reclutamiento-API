using DocumentFormat.OpenXml.Math;
using BCrypt.Net;

namespace Proyecto_Analisis_API.Helpers.Security
{
    public static class PasswordHasher
    {
        // workFactor 10–12 es razonable en web (11 es buen balance)
        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

        public static bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
