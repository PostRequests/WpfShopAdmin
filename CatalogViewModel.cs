using ConsoleShop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WpfShop.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly CatalogService _catalogService;
        private Product _selectedProduct;
        private string _selectedCategory;

        public CatalogViewModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
            LoadProducts();
        }

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterProductsByCategory();
            }
        }

        private void LoadProducts()
        {
            var allProducts = _catalogService.getAll();

            // Загружаем все товары
            Products.Clear();
            foreach (var product in allProducts)
            {
                Products.Add(product);
            }

            // Извлекаем уникальные категории
            Categories.Clear();
            var allCategories = allProducts
                .SelectMany(p => p.categories)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            Categories.Add("Все товары"); // Добавляем опцию "Все товары"
            foreach (var category in allCategories)
            {
                Categories.Add(category);
            }

            SelectedCategory = "Все товары";
        }

        private void FilterProductsByCategory()
        {
            var allProducts = _catalogService.getAll();

            Products.Clear();

            if (SelectedCategory == "Все товары" || string.IsNullOrEmpty(SelectedCategory))
            {
                // Показываем все товары
                foreach (var product in allProducts)
                {
                    Products.Add(product);
                }
            }
            else
            {
                // Фильтруем по выбранной категории
                var filteredProducts = allProducts
                    .Where(p => p.categories.Contains(SelectedCategory))
                    .ToList();

                foreach (var product in filteredProducts)
                {
                    Products.Add(product);
                }
            }

            // Выбираем первый товар для отображения
            SelectedProduct = Products.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}