using ConsoleShop;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfShop.ViewModels;

namespace WpfShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var userRepository = new JsonUserRepository("data/admins.json");
            var authService = new AuthService(userRepository);

            // Устанавливаем DataContext
            DataContext = new AuthViewModel(authService);

            // Привязка PasswordBox к ViewModel
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is AuthViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}