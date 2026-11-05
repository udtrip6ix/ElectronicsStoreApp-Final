using ElectronicsStoreApp;
using System;
using System.Windows;

namespace WpfApp2
{
    public partial class ProductEditWindow : Window
    {
        private int _productId = 0;

        public ProductEditWindow()
        {
            InitializeComponent();
            TitleText.Text = "Добавление товара";
        }

        public ProductEditWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            TitleText.Text = "Редактирование товара";

            using (var db = new ElectronicsStoreDBEntities())
            {
                var product = db.Products.Find(productId);
                if (product != null)
                {
                    NameBox.Text = product.Name;
                    CategoryBox.Text = product.Category;
                    PriceBox.Text = product.Price.ToString();
                    StockBox.Text = product.StockQuantity.ToString();
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("Заполните название и категорию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Некорректная цена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockBox.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Некорректное количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    if (_productId == 0)
                    {
                        var newProduct = new Products
                        {
                            Name = NameBox.Text.Trim(),
                            Category = CategoryBox.Text.Trim(),
                            Price = price,
                            StockQuantity = stock
                        };
                        db.Products.Add(newProduct);
                    }
                    else
                    {
                        var product = db.Products.Find(_productId);
                        if (product != null)
                        {
                            product.Name = NameBox.Text.Trim();
                            product.Category = CategoryBox.Text.Trim();
                            product.Price = price;
                            product.StockQuantity = stock;
                        }
                    }
                    db.SaveChanges();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}