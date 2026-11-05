using ElectronicsStoreApp;
using System.Linq;
using System.Windows;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new ElectronicsStoreDBEntities())
                {
                    var client = db.Clients.FirstOrDefault(c => c.Email == email && c.Password == password);

                    if (client == null)
                    {
                        MessageBox.Show("Неверный email или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var mainApp = new HomeWindow(client.Id, client.Role, client.FullName);
                    Application.Current.MainWindow = mainApp;
                    mainApp.Show();
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка БД: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegisterClick(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegisterWindow();
            regWindow.Owner = this;

            if (regWindow.ShowDialog() == true)
            {
                MessageBox.Show("Теперь вы можете войти под новым аккаунтом!", "Успех");
            }
        }
    }
}