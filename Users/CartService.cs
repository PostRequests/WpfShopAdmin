// CartService — операции с корзиной: добавить, удалить, получить строки, посчитать сумму.
namespace ConsoleShop
{
    // Одна строка корзины (для вывода/отчёта).
    public class CartLine
    {
        public Product product;   // товар, к которому относится строка
        public int quantity;      // выбранное пользователем количество
        public decimal sum;       // сумма по строке (price * quantity)

        public CartLine(Product product, int quantity, decimal sum)
        {
            this.product = product;
            this.quantity = quantity;
            this.sum = sum;
        }
    }

    public class CartService
    {
        private readonly IProductReadOnly catalog; // источник сведений о товарах
        private readonly Cart cart;                // состояние корзины (productId -> quantity)

        public CartService(IProductReadOnly catalog, Cart cart)
        {
            this.catalog = catalog;
            this.cart = cart;
        }

        // Добавляет товар в корзину.
        // Проверяет: положительное количество, наличие товара в каталоге, достаточный остаток на складе.
        public (bool ok, string? error) add(int productId, int quantity)
        {
            if (quantity <= 0)
                return (false, "количество должно быть больше 0");

            Product? found = catalog.getById(productId);
            if (found == null)
                throw new InvalidOperationException("Товар отсутствует в каталоге.");

            // сколько уже лежит в корзине для этого товара
            int currentQuantityInCart = 0;
            int existingQuantity;
            if (cart.items.TryGetValue(productId, out existingQuantity))
                currentQuantityInCart = existingQuantity;

            int newTotalQuantity = currentQuantityInCart + quantity;

            // если на складе ноль — добавить нельзя
            if (found.stock <= 0)
                throw new InvalidOperationException("Товар \"" + found.title + "\" закончился.");

            // не даём превысить остаток
            if (newTotalQuantity > found.stock)
                throw new InvalidOperationException("Недостаточно на складе. Доступно: " + found.stock + ", запрошено: " + newTotalQuantity + ".");

            // фиксируем новое количество
            cart.items[productId] = newTotalQuantity;
            return (true, null);
        }

        // Удаляет позицию из корзины.
        public (bool ok, string? error) remove(int productId)
        {
            bool removed = cart.items.Remove(productId);
            if (!removed)
                return (false, "товар не был в корзине");

            return (true, null);
        }

        // Полная очистка корзины.
        public void clear()
        {
            cart.clear();
        }

        // Возвращает список строк корзины с подсчитанными суммами.
        public List<CartLine> getLines()
        {
            List<CartLine> lines = new List<CartLine>();

            foreach (System.Collections.Generic.KeyValuePair<int, int> entry in cart.items)
            {
                int id = entry.Key;
                int quantity = entry.Value;

                Product? product = catalog.getById(id);
                if (product == null)
                    continue; // на случай рассинхронизации каталога и корзины

                decimal lineSum = product.price * quantity;
                lines.Add(new CartLine(product, quantity, lineSum));
            }

            return lines;
        }

        // Итоговая сумма по корзине.
        public decimal total()
        {
            decimal amount = 0m;

            foreach (System.Collections.Generic.KeyValuePair<int, int> entry in cart.items)
            {
                int id = entry.Key;
                int quantity = entry.Value;

                Product? product = catalog.getById(id);
                if (product == null)
                    continue;

                amount += product.price * quantity;
            }

            return amount;
        }
    }
}