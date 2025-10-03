using ConsoleShop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace WpfShop.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly CatalogService _catalogService;
        private Product _selectedProduct;
        private string _selectedCategory;
        private string _searchQuery = "";
        private CollectionViewSource _productsViewSource;

        public CatalogViewModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
            LoadProducts();
            InitializeViewSource();
        }

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        public ICollectionView FilteredProducts => _productsViewSource?.View;

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
                FilterProducts();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        // Команда очистки поиска
        private RelayCommand _clearSearchCommand;
        public RelayCommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ??
                  (_clearSearchCommand = new RelayCommand(obj =>
                  {
                      SearchQuery = "";
                  }));
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

            Categories.Add("Все товары");
            foreach (var category in allCategories)
            {
                Categories.Add(category);
            }

            SelectedCategory = "Все товары";
        }

        private void InitializeViewSource()
        {
            _productsViewSource = new CollectionViewSource { Source = Products };
            _productsViewSource.Filter += OnProductsFilter;
        }

        private void OnProductsFilter(object sender, FilterEventArgs e)
        {
            var product = e.Item as Product;
            if (product == null)
            {
                e.Accepted = false;
                return;
            }

            // Фильтрация по категории
            bool categoryMatch = SelectedCategory == "Все товары" ||
                               product.categories.Contains(SelectedCategory);

            // Фильтрация по поисковому запросу
            bool searchMatch = string.IsNullOrWhiteSpace(SearchQuery) ||
                             product.title.ToLower().Contains(SearchQuery.ToLower()) ||
                             (product.description ?? "").ToLower().Contains(SearchQuery.ToLower()) ||
                             product.tags.Any(tag => tag.ToLower().Contains(SearchQuery.ToLower()));

            e.Accepted = categoryMatch && searchMatch;
        }

        private void FilterProducts()
        {
            _productsViewSource?.View?.Refresh();

            // Автоматически выбираем первый товар после фильтрации
            if (FilteredProducts != null && FilteredProducts.Cast<Product>().Any())
            {
                SelectedProduct = FilteredProducts.Cast<Product>().First();
            }
            else
            {
                SelectedProduct = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}