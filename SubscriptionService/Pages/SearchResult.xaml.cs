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
using SubscriptionService.Data;

namespace SubscriptionService.Pages
{
    /// <summary>
    /// Логика взаимодействия для SearchResult.xaml
    /// </summary>
    public partial class SearchResult : Page
    {
        private string _currentChoiseTypeServices;
        private string _currentChoiseTypeBenefit;
        private string _currentSearch;
        public SearchResult(string search)
        {
            InitializeComponent();
            InitializationDropboxes();
            _currentSearch = search;
            _currentChoiseTypeServices = typeService.Text;
            _currentChoiseTypeBenefit = typeBenefit.Text;

            if (!Database.DBConnect.CheckRoleAdministration(Data.GlobalData.CurrentUser.id))
            {
                statisticsType.Visibility = Visibility.Hidden;
            }

            ShowResult(_currentSearch);
        }

        private void InitializationDropboxes()
        {
            typeService.Items.Clear();
            typeBenefit.Items.Clear();

            typeService.Items.Add("Все");
            typeBenefit.Items.Add("Все");

            List<string> colTypeService = Database.DBConnect.GetTypeServices();
            List<string> colTypeBenefit = Database.DBConnect.GetTypeBenefit();

            for (int i = 0; i < colTypeService.Count; i++)
                typeService.Items.Add(colTypeService[i]);

            for (int i = 0; i < colTypeBenefit.Count; i++)
                typeBenefit.Items.Add(colTypeBenefit[i]);

            typeService.SelectedIndex = 0;
            typeBenefit.SelectedIndex = 0;
        }

        /// <summary>
        /// Отобразить результат
        /// </summary>
        /// <param name="searchLine">Запрос поиска</param>
        private void ShowResult(string searchLine)
        {
            //Запаковка данных в класс
            Search search = new Search();
            search.Line = searchLine;
            search.TypeService = _currentChoiseTypeServices;
            search.TypeBenefit = _currentChoiseTypeBenefit;
            search.maxPrice = tb_maxPrice.Text;
            search.minPrice = tb_minPrice.Text;

            List<Service> resultFind = Database.DBConnect.GetSearchResult(search);
            SortPopularity(resultFind);
            ResultList.Children.Clear();

            if (resultFind.Count == 0) //Если ничего не найдено
            {
                CreateLableNotFound();
            }
            else
            {
                for (int i = 0; i < resultFind.Count; i++)
                {
                    CreateButton(resultFind[i]);
                }
            }
        }

        /// <summary>
        /// Сортировка по популярности
        /// </summary>
        private void SortPopularity(List<Service> list)
        {
            //Популярный сервис содержит в себе наибольшее кол-во подписок
            Tools.ListComparerServiceCountSub comparer = new Tools.ListComparerServiceCountSub();
            list.Sort(comparer);
        }

        /// <summary>
        /// Создание кнопки по форме из элементов
        /// </summary>
        /// <param name="result">Результат поиска</param>
        private void CreateButton(Service result)
        {
            Grid grid = ElementsWPF.Elements.CreatePanelResultSearch(result);
            grid.MouseLeftButtonDown += Button_MouseDown;
            grid.Name = $"E{result.Index.ToString("00000")}";
            ResultList.Children.Add(grid);
        }

        /// <summary>
        /// Создать сообщение о том, если ничего не найдено
        /// </summary>
        private void CreateLableNotFound()
        {
            Label message = new Label();
            message.Content = "По вашему запросу ничего не найдено";
            message.HorizontalAlignment = HorizontalAlignment.Center;
            message.VerticalAlignment = VerticalAlignment.Center;
            message.FontSize = 16;

            ResultList.Children.Add(message);
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;

            string index = grid.Name.Replace("E", "");
            MoreDetailedService mds = new MoreDetailedService(int.Parse(index));
            mds.ShowDialog();
        }

        //Событийный метод по закрытию dropdown
        private void typeService_DropDownClosed(object sender, EventArgs e)
        {
            if(((ComboBox)sender).Text != _currentChoiseTypeServices)
            {
                _currentChoiseTypeServices = ((ComboBox)sender).Text;
                ShowResult(_currentSearch);
            }
        }

        //Событийный метод по закрытию dropdown
        private void typeBenefit_DropDownClosed(object sender, EventArgs e)
        {
            if (((ComboBox)sender).Text != _currentChoiseTypeBenefit)
            {
                _currentChoiseTypeBenefit = ((ComboBox)sender).Text;
                ShowResult(_currentSearch);
            }
        }

        private void minPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            //Если ничего не было введено
            if (string.IsNullOrEmpty(tb.Text)) return;

            if (!string.IsNullOrEmpty(tb_maxPrice.Text))
            {
                int minPrice = int.Parse(tb.Text);
                int maxPrice = int.Parse(tb_maxPrice.Text);

                if (minPrice >= maxPrice)
                    tb_minPrice.Text = "";
            }

            ShowResult(_currentSearch);
        }

        private void maxPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            //Если ничего не было введено
            if (string.IsNullOrEmpty(tb.Text)) return;


            if (!string.IsNullOrEmpty(tb_minPrice.Text))
            {
                int maxPrice = int.Parse(tb.Text);
                int minPrice = int.Parse(tb_minPrice.Text);

                if (maxPrice <= minPrice)
                    tb_maxPrice.Text = "";
            }

            ShowResult(_currentSearch);
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowResult(_currentSearch);
            }
        }

        private void OnStaticType(object sender, RoutedEventArgs e)
        {
            StatisticType st = new StatisticType();
            st.ShowDialog();
        }

        private void OnInputOnlyDigit(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}
