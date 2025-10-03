// Интерфейс доступа к пользователям.
namespace ConsoleShop
{
    public interface IUserRepository
    {
        List<User> load();              // прочитать всех пользователей
        void save(List<User> users);    // перезаписать список пользователей
    }
}
