using ElectronicsStoreApp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace WpfApp2
{
    public partial class HomeWindow : Window
    {
        private int _clientId;
        private int _role;
        private List<CartItem> _cart = new List<CartItem>();

        public HomeWindow(int clientId, int role, string fullName)
        {
            InitializeComponent();
            _clientId = clientId;
            _role = role;
            ProfileText.Text = $"Пользователь: {fullName} ({(role == 1 ? "Администратор" : "Клиент")})";

            if (_role == 1)
            {
                AdminPanel.Visibility = Visibility.Visible;
            }

            LoadProducts();
            LoadClients();
            LoadOrders();
        }

        public class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal PricePerUnit { get; set; }
            public decimal TotalPrice => Quantity * PricePerUnit;
        }

        private void LoadProducts()
        {
            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var products = db.Products.ToList();
                    ProductsGrid.ItemsSource = products;
                    ProductComboBox.ItemsSource = products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message);
            }
        }

        private void LoadClients()
        {
            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var clients = db.Clients.ToList();
                    ClientComboBox.ItemsSource = clients;

                    if (_role == 1)
                    {
                        ClientComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        ClientComboBox.SelectedItem = clients.FirstOrDefault(c => c.Id == _clientId);
                        ClientComboBox.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var query = db.Orders.AsQueryable();
                    if (_role != 1)
                    {
                        query = query.Where(o => o.ClientId == _clientId);
                    }
                    OrdersGrid.ItemsSource = query.Include(o => o.Clients)
                                                  .OrderByDescending(o => o.OrderDate)
                                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        private void OrdersGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedOrder = OrdersGrid.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    OrderDetailsGrid.ItemsSource = db.OrderDetails
                        .Include(d => d.Products)
                        .Where(d => d.OrderId == selectedOrder.Id)
                        .ToList();
                }
            }
            else
            {
                OrderDetailsGrid.ItemsSource = null;
            }
        }

        private void AddToCartClick(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Укажите корректное количество!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = ProductComboBox.SelectedItem as Products;

            if (qty > product.StockQuantity)
            {
                MessageBox.Show($"На складе недостаточно товара! Доступно: {product.StockQuantity}", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingItem = _cart.FirstOrDefault(x => x.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = qty,
                    PricePerUnit = product.Price
                });
            }

            UpdateCart();
        }

        private void UpdateCart()
        {
            CartGrid.ItemsSource = null;
            CartGrid.ItemsSource = _cart;
            TotalPriceText.Text = $"{_cart.Sum(x => x.TotalPrice):N2} ₽";
        }

        private void ClearCartClick(object sender, RoutedEventArgs e)
        {
            _cart.Clear();
            UpdateCart();
        }

        private void CheckoutClick(object sender, RoutedEventArgs e)
        {
            if (!_cart.Any())
            {
                MessageBox.Show("Корзина пуста!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var client = ClientComboBox.SelectedItem as Clients;

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var order = new Orders
                    {
                        ClientId = client.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = _cart.Sum(x => x.TotalPrice)
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var item in _cart)
                    {
                        var detail = new OrderDetails
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            PricePerUnit = item.PricePerUnit
                        };
                        db.OrderDetails.Add(detail);

                        var productInDb = db.Products.Find(item.ProductId);
                        if (productInDb != null)
                        {
                            productInDb.StockQuantity -= item.Quantity;
                        }
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                _cart.Clear();
                UpdateCart();
                LoadProducts();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProductClick(object sender, RoutedEventArgs e)
        {
            var win = new ProductEditWindow();
            win.Owner = this;
            if (win.ShowDialog() == true) LoadProducts();
        }

        private void EditProductClick(object sender, RoutedEventArgs e)
        {
            var selected = ProductsGrid.SelectedItem as Products;
            if (selected == null)
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new ProductEditWindow(selected.Id);
            win.Owner = this;
            if (win.ShowDialog() == true) LoadProducts();
        }

        private void DeleteProductClick(object sender, RoutedEventArgs e)
        {
            var selected = ProductsGrid.SelectedItem as Products;
            if (selected == null)
            {
                MessageBox.Show("Выберите товар для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить товар '{selected.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    if (db.OrderDetails.Any(d => d.ProductId == selected.Id))
                    {
                        MessageBox.Show("Нельзя удалить товар, так как он присутствует в заказах!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var product = db.Products.Find(selected.Id);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                    }
                }
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void AccountClick(object sender, RoutedEventArgs e)
        {
            var profile = new ProfileWindow(_clientId);
            profile.Owner = this;

            if (profile.ShowDialog() == true)
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var client = db.Clients.Find(_clientId);
                    if (client != null)
                    {
                        ProfileText.Text = $"Пользователь: {client.FullName} ({(_role == 1 ? "Администратор" : "Клиент")})";
                    }
                }
            }
        }

        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            var auth = new MainWindow();
            Application.Current.MainWindow = auth;
            auth.Show();
            Close();
        }
    }
}