using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubscriptionService
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        MainForm _mainForm;
        public LoginForm()
        {
            InitializeComponent();
            Database.DBConnect.Connect();
        }

        private void OnLogIn(object sender, RoutedEventArgs e)
        {
            if (Database.DBConnect.FindLoginAndPass(login.Text, pass.Password))
            {
                _mainForm = new MainForm();
                _mainForm.Show();
                _mainForm.Owner = this.Owner;
                Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
    }
}
