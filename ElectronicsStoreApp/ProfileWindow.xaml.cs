using ElectronicsStoreApp;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace WpfApp2
{
    public partial class ProfileWindow : Window
    {
        private int _clientId;

        public ProfileWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;

            using (var db = new ElectronicsStoreDBEntities())
            {
                var client = db.Clients.Find(clientId);
                if (client != null)
                {
                    FullNameBox.Text = client.FullName;
                    EmailBox.Text = client.Email;
                    PhoneBox.Text = client.PhoneNumber;
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            EmailError.Text = PhoneError.Text = "";
            string fullName = FullNameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string password = PasswordBox.Password;
            bool isValid = true;

            if (fullName.Length < 5)
            {
                MessageBox.Show("ФИО слишком короткое!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailError.Text = "Некорректный email.";
                isValid = false;
            }

            if (!Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$"))
            {
                PhoneError.Text = "Некорректный телефон.";
                isValid = false;
            }

            if (!isValid) return;

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    if (db.Clients.Any(c => c.Id != _clientId && c.Email == email))
                    {
                        EmailError.Text = "Этот email уже занят другим пользователем.";
                        return;
                    }

                    var client = db.Clients.Find(_clientId);
                    if (client != null)
                    {
                        client.FullName = fullName;
                        client.Email = email;
                        client.PhoneNumber = phone;

                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            client.Password = password;
                        }

                        db.SaveChanges();
                    }
                }
                MessageBox.Show("Профиль успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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