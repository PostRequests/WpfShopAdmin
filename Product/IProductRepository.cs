// [Паттерн: Репозиторий]
// — Выделяет слой доступа к данным каталога, изолируя формат хранения (JSON) от остального кода.
// [Паттерн: Разделение интерфейсов]
// — IProductReadOnly (только чтение) и IProductRepository (чтение+запись).
//   Это облегчает повторное использование там, где нужна только «витрина» без прав изменения.
// [Принцип инверсии зависимостей (DIP)]
// — Сервисы зависят от абстракций (интерфейсов), а не от конкретной реализации.
namespace ConsoleShop
{
    public interface IProductReadOnly // объявляем интерфейс: IProductReadOnly
    {
        Product? getById(int id);
        List<Product> getAll();
        List<Product> search(string query);
    }

    public interface IProductRepository : IProductReadOnly // объявляем интерфейс: IProductRepository
    {
        List<Product> load();
        void save(List<Product> products);
    }
}
