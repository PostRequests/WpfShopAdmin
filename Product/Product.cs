// [Паттерн: Модель предметной области / Entity]
// — Простая структура данных без поведения.
// — Используется в репозитории и сервисах как переносимый объект данных.
namespace ConsoleShop
{
    public class Product // объявляем класс: Product// простые свойства на чтение/писание
    {
        public int id { get; set; }
        public string title { get; set; } = "";
        public List<string> tags { get; set; } = new();
        public string description { get; set; } = "";
        public int stock { get; set; }
        public decimal price { get; set; }
        public List<string> categories { get; set; } = new();     // Список категорий товара
        public List<string> imageUrls { get; set; } = new();      // Ссылки на изображения товара
    }
}