using ConsoleShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfShop.ViewModels;

namespace WpfShop
{
    /// <summary>
    /// Логика взаимодействия для CatalogViev.xaml
    /// </summary>
    public partial class CatalogViev : Window
    {
        public CatalogViev()
        {
            InitializeComponent();
            var productRepository = new JsonProductRepository("data/products.json");
            var catalogService = new CatalogService(productRepository);

            DataContext = new CatalogViewModel(catalogService);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат к окну авторизации
            var authWindow = new MainWindow();
            authWindow.Show();
            this.Close();
        }
    }
}
