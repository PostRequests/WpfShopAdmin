using ConsoleShop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
        private Brush _saveStatusColor = Brushes.Gray;

        // Новые свойства для работы с изображениями
        private int _currentImageIndex = 0;
        private List<string> _currentProductImages = new List<string>();
        private string _selectedProductImageUrls = "";
        private string _selectedProductTags = "";
        private string _selectedProductCategories = "";

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
                    SelectedProductImageUrls = string.Join(", ", _selectedProduct.imageUrls ?? new List<string>());
                    UpdateProductImages();
                }
                else
                {
                    SelectedProductTags = "";
                    SelectedProductCategories = "";
                    SelectedProductImageUrls = "";
                    _currentProductImages.Clear();
                    _currentImageIndex = 0;
                    UpdateImageProperties();
                }

                IsProductChanged = false;
                SaveStatus = "";
                OnPropertyChanged();
            }
        }

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

        public string SelectedProductImageUrls
        {
            get => _selectedProductImageUrls;
            set
            {
                _selectedProductImageUrls = value;
                OnPropertyChanged();
                UpdateImageUrlsFromString();
            }
        }

        // Свойства для работы с изображениями
        public string CurrentProductImage
        {
            get
            {
                if (_currentProductImages == null || _currentProductImages.Count == 0)
                    return null;

                if (_currentImageIndex >= 0 && _currentImageIndex < _currentProductImages.Count)
                {
                    var imagePath = _currentProductImages[_currentImageIndex];
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        // Преобразуем относительный путь в абсолютный
                        var fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "images", imagePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            return fullPath;
                        }
                    }
                }
                return null;
            }
        }

        public bool HasProductImages => _currentProductImages?.Count > 0;

        public bool CanGoToPreviousImage => HasProductImages && _currentImageIndex > 0;

        public bool CanGoToNextImage => HasProductImages && _currentImageIndex < _currentProductImages.Count - 1;

        public string ImageCounter => HasProductImages ? $"{_currentImageIndex + 1}/{_currentProductImages.Count}" : "";

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

        public Brush SaveStatusColor
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

        private void UpdateImageUrlsFromString()
        {
            if (_selectedProduct != null)
            {
                var newImageUrls = SelectedProductImageUrls
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(url => url.Trim())
                    .Where(url => !string.IsNullOrEmpty(url))
                    .ToList();

                if (!newImageUrls.SequenceEqual(_selectedProduct.imageUrls ?? new List<string>()))
                {
                    _selectedProduct.imageUrls = newImageUrls;
                    IsProductChanged = true;
                    UpdateProductImages();
                }
            }
        }

        private void UpdateProductImages()
        {
            _currentProductImages = _selectedProduct?.imageUrls?.Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
            _currentImageIndex = 0;
            UpdateImageProperties();
        }

        private void UpdateImageProperties()
        {
            OnPropertyChanged(nameof(CurrentProductImage));
            OnPropertyChanged(nameof(HasProductImages));
            OnPropertyChanged(nameof(CanGoToPreviousImage));
            OnPropertyChanged(nameof(CanGoToNextImage));
            OnPropertyChanged(nameof(ImageCounter));
        }

        // Команда очистки поиска
        private RelayCommand _clearSearchCommand;
        public RelayCommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ?? (_clearSearchCommand = new RelayCommand(obj =>
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
                return _saveProductCommand ?? (_saveProductCommand = new RelayCommand(obj =>
                {
                    SaveProductChanges();
                }));
            }
        }

        // Команда для предыдущего изображения
        private RelayCommand _previousImageCommand;
        public RelayCommand PreviousImageCommand
        {
            get
            {
                return _previousImageCommand ?? (_previousImageCommand = new RelayCommand(obj =>
                {
                    if (CanGoToPreviousImage)
                    {
                        _currentImageIndex--;
                        UpdateImageProperties();
                    }
                }));
            }
        }

        // Команда для следующего изображения
        private RelayCommand _nextImageCommand;
        public RelayCommand NextImageCommand
        {
            get
            {
                return _nextImageCommand ?? (_nextImageCommand = new RelayCommand(obj =>
                {
                    if (CanGoToNextImage)
                    {
                        _currentImageIndex++;
                        UpdateImageProperties();
                    }
                }));
            }
        }

        private void SaveProductChanges()
        {
            if (SelectedProduct != null && IsProductChanged)
            {
                try
                {
                    // Обновляем теги и категории из строк перед сохранением
                    UpdateTagsFromString();
                    UpdateCategoriesFromString();
                    UpdateImageUrlsFromString();

                    var result = _adminCatalogService.update(SelectedProduct);
                    if (result.ok)
                    {
                        SaveStatus = "✅ Изменения сохранены";
                        SaveStatusColor = Brushes.Green;
                        IsProductChanged = false;

                        // НЕ перезагружаем список - товар уже обновлен в памяти
                        // LoadProducts(); // Закомментировать эту строку

                        // Просто обновляем привязки
                        OnPropertyChanged(nameof(SelectedProduct));
                    }
                    else
                    {
                        SaveStatus = $"❌ Ошибка: {result.error}";
                        SaveStatusColor = Brushes.Red;
                    }
                }
                catch (Exception ex)
                {
                    SaveStatus = $"❌ Ошибка: {ex.Message}";
                    SaveStatusColor = Brushes.Red;
                }
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

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}