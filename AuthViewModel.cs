using ConsoleShop;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace WpfShop.ViewModels
{
    public class AuthViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _phone = "+7";
        private string _password = "";
        private string _statusMessage = "";
        private Brush _statusColor = Brushes.Gray;

        public AuthViewModel(AuthService authService)
        {
            _authService = authService;
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged("Phone");
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged("StatusMessage");
            }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged("StatusColor");
            }
        }

        // Команда входа
        private RelayCommand _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                  (_loginCommand = new RelayCommand(obj =>
                  {
                      var user = _authService.login(Phone, Password);

                      if (user != null)
                      {
                          StatusColor = Brushes.Green;

                          // Открываем каталог и закрываем текущее окно
                          OpenCatalogAndClose();
                      }
                      else
                      {
                          StatusColor = Brushes.Red;
                          StatusMessage = "❌ Ошибка входа: неверный номер телефона или пароль";
                      }
                  },
                  // CanExecute - проверка возможности выполнения команды
                  obj => !string.IsNullOrWhiteSpace(Phone) &&
                         !string.IsNullOrWhiteSpace(Password) &&
                         Phone.Length >= 11));
            }
        }

        // Открывает каталог, закрывает текущее окно
        private void OpenCatalogAndClose()
        {
            // Получаем текущее окно
            var currentWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (currentWindow != null)
            {
                // Создаем и показываем новое окно каталога
                var catalogWindow = new CatalogViev();
                catalogWindow.Show();

                // Закрываем текущее окно авторизации
                currentWindow.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}