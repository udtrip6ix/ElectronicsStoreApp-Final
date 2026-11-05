using ElectronicsStoreApp;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace WpfApp2
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterClick(object sender, RoutedEventArgs e)
        {
            FullNameError.Text = EmailError.Text = PhoneError.Text = PasswordError.Text = RepeatError.Text = "";

            string fullName = FullNameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string password = PasswordBox.Password;
            string repeat = PasswordRepeatBox.Password;
            bool isValid = true;

            if (fullName.Length < 5)
            {
                FullNameError.Text = "ФИО слишком короткое.";
                isValid = false;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailError.Text = "Некорректный email.";
                isValid = false;
            }

            if (!Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$"))
            {
                PhoneError.Text = "Некорректный номер телефона.";
                isValid = false;
            }

            if (password.Length < 6)
            {
                PasswordError.Text = "Пароль должен быть не менее 6 символов.";
                isValid = false;
            }

            if (password != repeat)
            {
                RepeatError.Text = "Пароли не совпадают.";
                isValid = false;
            }

            if (isValid)
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    if (db.Clients.Any(c => c.Email == email))
                    {
                        EmailError.Text = "Этот email уже занят.";
                        isValid = false;
                    }
                }
            }

            if (!isValid) return;

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var newClient = new Clients
                    {
                        FullName = fullName,
                        Email = email,
                        PhoneNumber = phone,
                        Password = password,
                        Role = 2
                    };

                    db.Clients.Add(newClient);
                    db.SaveChanges();
                }

                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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