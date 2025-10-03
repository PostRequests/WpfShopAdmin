// PasswordHasher � ������� SHA-256 �� ������ � ���������� hex-������ � ������ ��������.
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
