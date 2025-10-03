using ConsoleShop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System;

namespace WpfShop.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly CatalogService _catalogService;
        private readonly AdminCatalogService _adminCatalogService;
        private Product _selectedProduct;
        private string _selectedCategory;
        private string _searchQuery = "";
        private CollectionViewSource _productsViewSource;
        private bool _isProductChanged = false;
        private string _saveStatus = "";
        private System.Windows.Media.Brush _saveStatusColor = System.Windows.Media.Brushes.Gray;

        public CatalogViewModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
            var productRepository = new JsonProductRepository("data/products.json");
            _adminCatalogService = new AdminCatalogService(productRepository);
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
                if (_selectedProduct != null)
                {
                    _selectedProduct.PropertyChanged -= SelectedProduct_PropertyChanged;
                }

                _selectedProduct = value;

                if (_selectedProduct != null)
                {
                    _selectedProduct.PropertyChanged += SelectedProduct_PropertyChanged;
                    SelectedProductTags = string.Join(", ", _selectedProduct.tags ?? new List<string>());
                    SelectedProductCategories = string.Join(", ", _selectedProduct.categories ?? new List<string>());
                }
                else
                {
                    SelectedProductTags = "";
                    SelectedProductCategories = "";
                }

                IsProductChanged = false;
                SaveStatus = "";
                OnPropertyChanged();
            }
        }

        private string _selectedProductTags = "";
        public string SelectedProductTags
        {
            get => _selectedProductTags;
            set
            {
                _selectedProductTags = value;
                OnPropertyChanged();
                UpdateTagsFromString();
            }
        }

        private string _selectedProductCategories = "";
        public string SelectedProductCategories
        {
            get => _selectedProductCategories;
            set
            {
                _selectedProductCategories = value;
                OnPropertyChanged();
                UpdateCategoriesFromString();
            }
        }

        public bool IsProductChanged
        {
            get => _isProductChanged;
            set
            {
                _isProductChanged = value;
                OnPropertyChanged();
            }
        }

        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Media.Brush SaveStatusColor
        {
            get => _saveStatusColor;
            set
            {
                _saveStatusColor = value;
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

        private void SelectedProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsProductChanged = true;
        }

        private void UpdateTagsFromString()
        {
            if (_selectedProduct != null)
            {
                var newTags = SelectedProductTags
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToList();

                if (!newTags.SequenceEqual(_selectedProduct.tags ?? new List<string>()))
                {
                    _selectedProduct.tags = newTags;
                    IsProductChanged = true;
                }
            }
        }

        private void UpdateCategoriesFromString()
        {
            if (_selectedProduct != null)
            {
                var newCategories = SelectedProductCategories
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(cat => cat.Trim())
                    .Where(cat => !string.IsNullOrEmpty(cat))
                    .ToList();

                if (!newCategories.SequenceEqual(_selectedProduct.categories ?? new List<string>()))
                {
                    _selectedProduct.categories = newCategories;
                    IsProductChanged = true;
                }
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

        // Команда сохранения товара
        private RelayCommand _saveProductCommand;
        public RelayCommand SaveProductCommand
        {
            get
            {
                return _saveProductCommand ??
                  (_saveProductCommand = new RelayCommand(obj =>
                  {
                      if (SelectedProduct != null && IsProductChanged)
                      {
                          try
                          {
                              var result = _adminCatalogService.update(SelectedProduct);
                              if (result.ok)
                              {
                                  SaveStatus = "✅ Изменения сохранены";
                                  SaveStatusColor = System.Windows.Media.Brushes.Green;
                                  IsProductChanged = false;

                                  // Обновляем список продуктов
                                  LoadProducts();
                              }
                              else
                              {
                                  SaveStatus = $"❌ Ошибка: {result.error}";
                                  SaveStatusColor = System.Windows.Media.Brushes.Red;
                              }
                          }
                          catch (Exception ex)
                          {
                              SaveStatus = $"❌ Ошибка: {ex.Message}";
                              SaveStatusColor = System.Windows.Media.Brushes.Red;
                          }
                      }
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