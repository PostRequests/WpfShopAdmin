using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConsoleShop
{
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _title = "";
        private List<string> _tags = new();
        private string _description = "";
        private int _stock;
        private decimal _price;
        private List<string> _categories = new();
        private List<string> _imageUrls = new();

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public List<string> tags
        {
            get => _tags;
            set { _tags = value; OnPropertyChanged(); }
        }

        public string description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public int stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public decimal price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public List<string> categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public List<string> imageUrls
        {
            get => _imageUrls;
            set { _imageUrls = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}