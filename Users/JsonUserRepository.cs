// JsonUserRepository — хранит пользователей в JSON-файле (одним списком).
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.IO;
namespace ConsoleShop
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string filePath;            // путь к файлу users.json
        private readonly JsonSerializerOptions json; // настройки сериализации

        public JsonUserRepository(string jsonPath)
        {
            filePath = jsonPath;

            // читаемый JSON
            json = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        // Чтение: если файл есть — читаем → десериализуем → отдаём список; иначе — пустой список.
        public List<User> load()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<User>();

                string content = File.ReadAllText(filePath, Encoding.UTF8);
                List<User>? parsed = System.Text.Json.JsonSerializer.Deserialize<List<User>>(content, json);
                return parsed ?? new List<User>();
            }
            catch
            {
                // повреждённый файл/ошибка ввода-вывода → отдаём пустой список
                return new List<User>();
            }
        }

        // Запись: сериализуем → пишем во временный файл → заменяем основной (защита от обрыва записи).
        public void save(List<User> users)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string content = System.Text.Json.JsonSerializer.Serialize(users, json);
            string tempPath = filePath + ".tmp";

            File.WriteAllText(tempPath, content, new UTF8Encoding(false));
            File.Move(tempPath, filePath, true);
        }
    }
}
