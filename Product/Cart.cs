// [Модель состояния сеанса пользователя]
// — Корзина — простая сущность, хранящая выбранные позиции (productId -> qty).
// — Не является отдельным «паттерном» GoF, но это типичная предметная модель,
//   живущая в памяти во время сессии (не сохраняется на диск).
namespace ConsoleShop
{
    public class Cart
    {
        public Dictionary<int, int> items { get; } = new();
        public void clear() => items.Clear(); // метод clear — вернём void
    }
}
