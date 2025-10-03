// ===============================
// [Паттерн: Сервисный слой]
// — Тонкий фасад для операций «чтения каталога».
// — Зависит от IProductReadOnly: лимитируем права на уровне типов.
// — [Внедрение зависимостей] через конструктор (без фреймворков).
// ===============================
namespace ConsoleShop
{
    public sealed class CatalogService // объявляем класс (запрещаем наследование): CatalogService
    {
        private readonly IProductReadOnly repo;
        public CatalogService(IProductReadOnly repo) { this.repo = repo; } // конструктор CatalogService — передаём зависимости через параметры (IProductReadOnly repo)

        public List<Product> getAll() => repo.getAll(); // метод getAll — вернём List<Product>
        public Product? getById(int id) => repo.getById(id); // метод getById — вернём Product?
        public List<Product> search(string query) => repo.search(query); // метод search — вернём List<Product>
    }
}