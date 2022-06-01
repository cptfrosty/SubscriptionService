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
using System.Windows.Shapes;
using SubscriptionService.Pages;

namespace SubscriptionService
{
    /// <summary>
    /// Логика взаимодействия для MainForm.xaml
    /// </summary>
    public partial class MainForm : Window
    {
        public MainForm()
        {
            InitializeComponent();
            InitialFirstData();
            PageController.Content = new PersonalAccount();
        }

        /// <summary>
        /// Инициализация первых данных
        /// </summary>
        private void InitialFirstData()
        {
            NameUser.Content = Data.GlobalData.CurrentUser.fio;
            idUser.Content = $"id {Data.GlobalData.CurrentUser.id.ToString("00000")}";
        }

        private void OnSearch(object sender, RoutedEventArgs e)
        {
            if (PageController.CanGoBack)
                PageController.RemoveBackEntry();

            PageController.Content = null;
            PageController.Content = new SearchResult(searchPoly.Text);
        }

        private void OnLK(object sender, RoutedEventArgs e)
        {
            if (PageController.CanGoBack)
                PageController.RemoveBackEntry();
            
            PageController.Content = new PersonalAccount();
        }

        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
