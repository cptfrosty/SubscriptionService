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

namespace SubscriptionService.Pages
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccount.xaml
    /// </summary>
    public partial class PersonalAccount : Page
    {
        //Личный кабинет пользователя
        public PersonalAccount()
        {
            InitializeComponent();
            UpdatePage();
        }

        private void UpdatePage()
        {
            LoadInfoDatabase();
            ListSubscribe.Children.Clear();

            List<Data.UserSubscriptions> subService =
                Database.DBConnect.GetSubscriptionServiceUser(Data.GlobalData.CurrentUser.id);

            if (subService.Count == 0)
            {
                Label empty = new Label();
                empty.Content = "У Вас отсутствуют подписки на сервисы.";
                ListSubscribe.Children.Add(empty);
            }
            else
            {
                for (int i = 0; i < subService.Count; i++)
                {
                    Button btn = new Button();
                    ListSubscribe.Children.Add(ElementsWPF.Elements.CreatePanelSubLK(subService[i], ref btn));
                    btn.Click += OnUnsubscribe;
                }
            }
        }

        /// <summary>
        /// Загрузка информации из базы данных
        /// </summary>
        private void LoadInfoDatabase()
        {
            nameUser.Content = Data.GlobalData.CurrentUser.fio;
            roleUser.Content = $"Роль: {Database.DBConnect.GetRole(Data.GlobalData.CurrentUser.idRole)}";
            benefitsUser.Content = $"Льготы:\n{Database.DBConnect.GetBenefitsName(Data.GlobalData.CurrentUser.id)}";
        }

        private void OnUnsubscribe(object sender, RoutedEventArgs e)
        {
            string index = ((Button)sender).Name.Replace("E", "");
            Database.DBConnect.UnsubscribeService(Data.GlobalData.CurrentUser.id, long.Parse(index));
            MessageBox.Show("Вы успешно отписались от сервиса!");
            UpdatePage();
        }
    }
}
