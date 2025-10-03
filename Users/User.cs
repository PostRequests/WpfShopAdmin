// Модель пользователя.
namespace ConsoleShop
{
    public class User
    {
        public string name { get; set; } = "";         // имя
        public string phone { get; set; } = "";        // телефон (8XXXXXXXXXX)
        public string passwordHash { get; set; } = ""; // SHA-256 от пароля
        public bool isAdmin { get; set; }              // права администратора
    }
}
