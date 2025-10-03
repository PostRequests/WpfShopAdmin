using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
namespace ConsoleShop
{
    // [Паттерн: Репозиторий]
    // — Инкапсулирует доступ к JSON (чтение/запись каталога), отделяя доменную логику от хранилища.
    // [Техника: атомарная запись файла]
    // — Записываем во временный файл .tmp и затем делаем Move/Replace: снижение риска порчи данных.
    // [Заметка об удобочитаемости]
    public sealed class JsonProductRepository : IProductRepository // объявляем класс (запрещаем наследование): JsonProductRepository
    {
        private readonly string path;
        private readonly JsonSerializerOptions options;

        public JsonProductRepository(string jsonPath) // конструктор JsonProductRepository — передаём зависимости через параметры (string jsonPath)
        {
            path = jsonPath;
            options = new JsonSerializerOptions
            {
                WriteIndented = true,

                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public List<Product> load() // метод load — вернём List<Product>
        {
            try
            {
                if (!File.Exists(path))
                    return new List<Product>(); // возвращаем результат из функции

                string json = File.ReadAllText(path, Encoding.UTF8); // читаем весь файл в строку
                var data = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>(); // распаковываем JSON в объекты

                // Стабильная сортировка по id
                data.Sort((a, b) => a.id.CompareTo(b.id)); // приводим список к стабильному порядку (по id)
                return data; //результат
            }
            catch
            {
                // [Защита от повреждённого файла] Возвращаем пустой список
                return new List<Product>(); // возвращаем результат из функции
            }
        }

        public void save(List<Product> products) // метод save — вернём void
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!); // на всякий случай убеждаемся, что папка есть

            // Стабильная сортировка перед записью
            products.Sort((a, b) => a.id.CompareTo(b.id)); // приводим список к стабильному порядку (по id)

            string json = JsonSerializer.Serialize(products, options); // упаковываем объекты обратно в JSON
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json, new UTF8Encoding(false)); // сохраняем строку в файл
            File.Move(tmp, path, true); // атомарная замена // аккуратно подменяем старый файл новым
        }

        public Product? getById(int id) // метод getById — вернём Product?
        {
            List<Product> all = load();
            for (int i = 0; i < all.Count; i++) //перебираем элементы
            {
                if (all[i].id == id) return all[i];
            }
            return null;
        }

        public List<Product> getAll() // метод getAll — вернём List<Product>
        {
            return load();
        }

        public List<Product> search(string query) // метод search — вернём List<Product>
        {
            string q = (query ?? string.Empty).Trim().ToLowerInvariant(); // приводим к нижнему регистру без влияния локали
            List<Product> all = load();
            if (q.Length == 0) return all;

            List<Product> result = new List<Product>();
            for (int i = 0; i < all.Count; i++) //перебираем элементы
            {
                Product p = all[i];

                string title = (p.title ?? string.Empty).ToLowerInvariant(); // приводим к нижнему регистру без влияния локали
                string desc = (p.description ?? string.Empty).ToLowerInvariant(); // приводим к нижнему регистру без влияния локали

                bool match = false;

                // [Фильтрация по полям]
                if (title.Contains(q) || desc.Contains(q))
                {
                    match = true;
                }
                else
                {
                    List<string> tags = p.tags ?? new List<string>();
                    for (int t = 0; t < tags.Count; t++) //перебираем элементы
                    {
                        string tag = (tags[t] ?? string.Empty).ToLowerInvariant(); // приводим к нижнему регистру без влияния локали
                        if (tag.Contains(q)) { match = true; break; }
                    }

                    // Поиск по категориям
                    if (!match)
                    {
                        List<string> categories = p.categories ?? new List<string>();
                        for (int c = 0; c < categories.Count; c++) //перебираем элементы
                        {
                            string category = (categories[c] ?? string.Empty).ToLowerInvariant(); // приводим к нижнему регистру без влияния локали
                            if (category.Contains(q)) { match = true; break; }
                        }
                    }
                }

                if (match) result.Add(p);
            }
            return result; // возвращаем результат из функции
        }
    }
}