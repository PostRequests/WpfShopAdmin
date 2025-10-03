// [Паттерн: Сервисный слой]
// — Командные операции каталога (создать/обновить/удалить) поверх репозитория.
// — Логика генерации id и проверок вынесена из UI.
// — Внедрение зависимостей через конструктор (интерфейс репозитория).
namespace ConsoleShop
{
    public sealed class AdminCatalogService // объявляем класс (запрещаем наследование): AdminCatalogService
    {
        private readonly IProductRepository repo;
        public AdminCatalogService(IProductRepository repo) { this.repo = repo; } // конструктор AdminCatalogService — передаём зависимости через параметры (IProductRepository repo)

        public Product create(Product p) // метод create — вернём Product
        {
            List<Product> all = repo.load();

            // [Инвариант] Если id не задан, присваиваем следующий вручную.
            int maxId = 0;
            if (p.id == 0)
            {
                for (int i = 0; i < all.Count; i++) //перебираем элементы
                {
                    if (all[i].id > maxId) maxId = all[i].id;
                }
                p.id = maxId + 1;
            }

            // Инициализация списков, если они null
            p.tags = p.tags ?? new List<string>();
            p.categories = p.categories ?? new List<string>();
            p.imageUrls = p.imageUrls ?? new List<string>();

            all.Add(p); // докидываем элемент в коллекцию
            repo.save(all);
            return p; // возвращаем результат из функции
        }

        public (bool ok, string? error) update(Product p) // метод update — вернём (bool ok, string? error)
        {
            List<Product> all = repo.load();

            // [Проверка существования] Ищем индекс по id
            int index = -1;
            for (int i = 0; i < all.Count; i++) //перебираем элементы
            {
                if (all[i].id == p.id) { index = i; break; }
            }
            if (index < 0) return (false, "товар не найден");

            // Инициализация списков, если они null
            p.tags = p.tags ?? new List<string>();
            p.categories = p.categories ?? new List<string>();
            p.imageUrls = p.imageUrls ?? new List<string>();

            all[index] = p;
            repo.save(all);
            return (true, null); // возвращаем результат из функции
        }

        public (bool ok, string? error) delete(int id) // метод delete — вернём (bool ok, string? error)
        {
            List<Product> all = repo.load();

            // [Удаление по ключу] Без RemoveAll — первый найденный элемент
            bool removed = false;
            for (int i = 0; i < all.Count; i++) //перебираем элементы
            {
                if (all[i].id == id)
                {
                    all.RemoveAt(i);
                    removed = true;
                    break; // выходим из цикла досрочно
                }
            }
            if (!removed) return (false, "товар не найден");

            repo.save(all);
            return (true, null); // возвращаем результат из функции
        }
    }
}