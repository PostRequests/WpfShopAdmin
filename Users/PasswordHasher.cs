// PasswordHasher — считает SHA-256 от пароля и возвращает hex-строку в нижнем регистре.
using System.Security.Cryptography;
using System.Text;

namespace ConsoleShop
{
    public static class PasswordHasher
    {
        public static string hash(string password)
        {
            using SHA256 sha = SHA256.Create();

            byte[] pass = Encoding.UTF8.GetBytes(password);
            byte[] digest = sha.ComputeHash(pass);

            string hex = Convert.ToHexString(digest).ToLowerInvariant();
            return hex;
        }
    }
}
