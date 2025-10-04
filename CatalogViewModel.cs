using ConsoleShop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfShop.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        
        private readonly CatalogService _catalogService;// Сервис для чтения каталога
        private readonly AdminCatalogService _adminCatalogService;// Сервис для администрирования каталога
        private Product _selectedProduct; // Текущий выбранный товар в списке
        private string _selectedCategory;// Название категории, выбранной в фильтре 
        private string _searchQuery = "";// Текущее значение поискового запроса 
        private CollectionViewSource _productsViewSource;   // Источник данных для фильтрации и отображения списка товаров
        private bool _isProductChanged; // Флаг: были ли внесены изменения в данные выбранного товара
        private string _saveStatus = "";  // Текстовое сообщение о результате последней операции
        private Brush _saveStatusColor = Brushes.Gray;// Цвет отображения статусного сообщения (зелёный — успех, красный — ошибка)

        // Состояние изображений выбранного товара
        private int _currentImageIndex = 0;
        private List<string> _currentProductImages = new List<string>();
        private string _selectedProductImageUrls = "";
        private string _selectedProductTags = "";
        private string _selectedProductCategories = "";
        private int _selectedProductId;

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
                    _selectedProduct.PropertyChanged -= SelectedProduct_PropertyChanged;

                _selectedProduct = value;
                _selectedProductId = _selectedProduct?.id ?? 0;

                if (_selectedProduct != null)
                {
                    _selectedProduct.PropertyChanged += SelectedProduct_PropertyChanged;
                    SelectedProductTags = string.Join(", ", _selectedProduct.tags ?? Enumerable.Empty<string>());
                    SelectedProductCategories = string.Join(", ", _selectedProduct.categories ?? Enumerable.Empty<string>());
                    SelectedProductImageUrls = string.Join(", ", _selectedProduct.imageUrls ?? Enumerable.Empty<string>());
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

        public string CurrentProductImage
        {
            get
            {
                if (_currentProductImages.Count == 0)
                    return null;

                if (_currentImageIndex >= 0 && _currentImageIndex < _currentProductImages.Count)
                {
                    var imagePath = _currentProductImages[_currentImageIndex];
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        var fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "images", imagePath);
                        if (System.IO.File.Exists(fullPath))
                            return fullPath;
                    }
                }
                return null;
            }
        }

        public bool HasProductImages => _currentProductImages.Count > 0;
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

        // Отслеживает изменения свойств товара для активации кнопки "Сохранить"
        private void SelectedProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsProductChanged = true;
        }

        private void UpdateTagsFromString()
        {
            if (_selectedProduct == null) return;

            var newTags = ParseCommaSeparatedList(SelectedProductTags);
            if (!newTags.SequenceEqual(_selectedProduct.tags ?? Enumerable.Empty<string>()))
            {
                _selectedProduct.tags = newTags;
                IsProductChanged = true;
            }
        }

        private void UpdateCategoriesFromString()
        {
            if (_selectedProduct == null) return;

            var newCategories = ParseCommaSeparatedList(SelectedProductCategories);
            if (!newCategories.SequenceEqual(_selectedProduct.categories ?? Enumerable.Empty<string>()))
            {
                _selectedProduct.categories = newCategories;
                IsProductChanged = true;
            }
        }

        private void UpdateImageUrlsFromString()
        {
            if (_selectedProduct == null) return;

            var newImageUrls = ParseCommaSeparatedList(SelectedProductImageUrls);
            if (!newImageUrls.SequenceEqual(_selectedProduct.imageUrls ?? Enumerable.Empty<string>()))
            {
                _selectedProduct.imageUrls = newImageUrls;
                IsProductChanged = true;
                UpdateProductImages();
            }
        }

        private List<string> ParseCommaSeparatedList(string input)
        {
            return input
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
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

        // Команды
        public ICommand ClearSearchCommand => new RelayCommand(_ => SearchQuery = "");
        public ICommand SaveProductCommand => new RelayCommand(_ => SaveProductChanges());
        public ICommand PreviousImageCommand => new RelayCommand(_ =>
        {
            if (CanGoToPreviousImage)
            {
                _currentImageIndex--;
                UpdateImageProperties();
            }
        });
        public ICommand NextImageCommand => new RelayCommand(_ =>
        {
            if (CanGoToNextImage)
            {
                _currentImageIndex++;
                UpdateImageProperties();
            }
        });
        public ICommand AddNewProductCommand => new RelayCommand(_ => AddNewProduct());
        public ICommand DeleteProductCommand => new RelayCommand(
            _ => DeleteProduct(),
            _ => SelectedProduct != null
        );

        private void SaveProductChanges()
        {
            if (SelectedProduct == null || !IsProductChanged) return;

            try
            {
                int currentProductId = SelectedProduct.id;

                // Синхронизируем данные из текстовых полей
                UpdateTagsFromString();
                UpdateCategoriesFromString();
                UpdateImageUrlsFromString();

                var result = _adminCatalogService.update(SelectedProduct);
                if (result.ok)
                {
                    SaveStatus = "✅ Изменения сохранены";
                    SaveStatusColor = Brushes.Green;
                    IsProductChanged = false;
                    LoadProductsAndRestoreSelection(currentProductId);
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

        private void AddNewProduct()
        {
            try
            {
                var newProduct = new Product
                {
                    title = "Новый товар",
                    description = "Описание товара",
                    price = 0,
                    stock = 0,
                    tags = new List<string>(),
                    categories = new List<string>(),
                    imageUrls = new List<string>()
                };

                var createdProduct = _adminCatalogService.create(newProduct);
                LoadProducts();
                SelectedProduct = Products.FirstOrDefault(p => p.id == createdProduct.id);

                SaveStatus = "✅ Новый товар создан";
                SaveStatusColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                SaveStatus = $"❌ Ошибка при создании товара: {ex.Message}";
                SaveStatusColor = Brushes.Red;
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить товар \"{SelectedProduct.title}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var deleteResult = _adminCatalogService.delete(SelectedProduct.id);
                if (deleteResult.ok)
                {
                    SaveStatus = "✅ Товар удален";
                    SaveStatusColor = Brushes.Green;
                    LoadProducts();
                }
                else
                {
                    SaveStatus = $"❌ Ошибка при удалении: {deleteResult.error}";
                    SaveStatusColor = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                SaveStatus = $"❌ Ошибка при удалении: {ex.Message}";
                SaveStatusColor = Brushes.Red;
            }
        }

        // Загружает товары и восстанавливает выделение по ID
        private void LoadProductsAndRestoreSelection(int productIdToSelect = 0)
        {
            var allProducts = _catalogService.getAll();

            Products.Clear();
            foreach (var product in allProducts)
                Products.Add(product);

            // Обновляем список категорий
            Categories.Clear();
            Categories.Add("Все товары");
            var uniqueCategories = allProducts
                .SelectMany(p => p.categories ?? Enumerable.Empty<string>())
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c);
            foreach (var category in uniqueCategories)
                Categories.Add(category);

            // Восстанавливаем выделение
            if (productIdToSelect > 0)
            {
                SelectedProduct = Products.FirstOrDefault(p => p.id == productIdToSelect);
                if (SelectedProduct == null)
                    SelectedCategory = "Все товары";
            }
            else
            {
                SelectedCategory = "Все товары";
            }
        }

        private void LoadProducts()
        {
            LoadProductsAndRestoreSelection(_selectedProductId);
        }

        private void InitializeViewSource()
        {
            _productsViewSource = new CollectionViewSource { Source = Products };
            _productsViewSource.Filter += OnProductsFilter;
        }

        // Фильтрация товаров по категории и поисковому запросу
        private void OnProductsFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not Product product)
            {
                e.Accepted = false;
                return;
            }

            bool categoryMatch = SelectedCategory == "Все товары" ||
                                 (product.categories?.Contains(SelectedCategory) == true);

            bool searchMatch = string.IsNullOrWhiteSpace(SearchQuery) ||
                               product.title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                               (product.description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                               (product.tags?.Any(tag => tag.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) == true);

            e.Accepted = categoryMatch && searchMatch;
        }

        private void FilterProducts()
        {
            _productsViewSource?.View?.Refresh();

            // Автоматически выбираем первый товар после фильтрации
            SelectedProduct = FilteredProducts?.Cast<Product>().FirstOrDefault();
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
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}