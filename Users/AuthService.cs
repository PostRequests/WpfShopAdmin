// AuthService — регистрация и вход пользователей.
namespace ConsoleShop
{
    public class AuthService
    {
        private readonly IUserRepository repository; // источник/приёмник данных пользователей

        public AuthService(IUserRepository repository)
        {
            this.repository = repository;
        }

        // Регистрация: нормализуем номер → валидируем поля → проверяем уникальность → считаем хеш пароля → сохраняем запись.
        public (bool ok, string? error) register(string name, string phoneRaw, string password)
        {
            // приводим телефон к виду 8XXXXXXXXXX
            string? normalizedPhone = PhoneUtil.normalize(phoneRaw);
            if (normalizedPhone == null)
                return (false, "номер телефона выглядит некорректно");

            // минимальная валидация имени/пароля
            string preparedName = (name ?? string.Empty).Trim();
            if (preparedName.Length < 2)
                return (false, "имя слишком короткое");
            if (string.IsNullOrEmpty(password))
                return (false, "пароль пустой");

            // читаем текущих пользователей
            List<User> users = repository.load();

            // проверяем уникальность номера
            bool phoneAlreadyExists = false;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].phone == normalizedPhone)
                {
                    phoneAlreadyExists = true;
                    break;
                }
            }
            if (phoneAlreadyExists)
                return (false, "пользователь с таким номером уже существует");

            // считаем хеш пароля 
            string passwordHashHex = PasswordHasher.hash(password);

            // записываем нового пользователя
            User newUser = new User
            {
                name = preparedName,
                phone = normalizedPhone,
                passwordHash = passwordHashHex,
                isAdmin = false
            };
            users.Add(newUser);
            repository.save(users);

            return (true, null);
        }

        // Вход: нормализуем номер → находим пользователя → пересчитываем хеш введённого пароля и сравниваем.
        public User? login(string phoneRaw, string password)
        {
            string? normalizedPhone = PhoneUtil.normalize(phoneRaw);
            if (normalizedPhone == null)
                return null;

            List<User> users = repository.load();

            // ищем пользователя по номеру
            User? candidate = null;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].phone == normalizedPhone)
                {
                    candidate = users[i];
                    break;
                }
            }
            if (candidate == null)
                return null;

            // сверяем хеши
            string checkHash = PasswordHasher.hash(password);
            if (checkHash == candidate.passwordHash)
                return candidate;

            return null;
        }
    }
}
