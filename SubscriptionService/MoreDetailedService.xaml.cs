using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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

namespace SubscriptionService.Pages
{
    /// <summary>
    /// Логика взаимодействия для MoreDetailedService.xaml
    /// </summary>
    public partial class MoreDetailedService : Window
    {
        private long _currentIndex;
        private Data.Service _currentService;
        private long _discount = 0; //Скидка пользователю
        private long _actualPriceWithDelivery; //Актуальная цена с доставкой
        private long _actualPriceWithoutDelivery; //Актуальная цена без доставки

        private bool isSigned = false; //Подписан
        public MoreDetailedService(long index)
        {
            InitializeComponent();
            
            _currentIndex = index;
            FillStructurePage();

            if (Database.DBConnect.CheckRoleAdministration(Data.GlobalData.CurrentUser.id))
            {
                CreateDiagram();
            }
            else
            {
                Stats.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Заполнение структуры страницы
        /// </summary>
        private void FillStructurePage()
        {
            //льготы сервиса
            List<Data.BenefitService> benefits = new List<Data.BenefitService>();
            Data.Service service = Database.DBConnect.GetService(_currentIndex, ref benefits);

            _currentService = service;

            indexService.Content = $"F{service.Index.ToString("00000")}";
            nameService.Content = $"{service.Name}";
            typeService.Content = $"{service.Type}";

            //Проверка подписки
            if (Database.DBConnect.CheckSubService(Data.GlobalData.CurrentUser.id, service.Index))
            {
                //Если пользователь подписан, 
                //то заблокировать возможность повторной подписки
                isSigned = true;
                nameService.Content += " (в подписках)";

                subWithDelivery.IsEnabled = false;
                subWithoutDelivery.IsEnabled = false;
                termWithDelivery.IsEnabled = false;
                termWithoutDelivery.IsEnabled = false;
            }

            //Если есть льготы в сервисе и у пользователя
            _discount = FindDiscount(benefits);
            if (_discount > 0) 
            {
                //Посчитать актуальную цену
                _actualPriceWithDelivery = 
                    service.PriceWithDelivery - CalcDiscount(service.PriceWithDelivery, _discount);
                _actualPriceWithoutDelivery = 
                    service.PriceWithoutDelivery - CalcDiscount(service.PriceWithoutDelivery, _discount);

                //Отобразить актуальную цену
                priceWithDilivery.Content = 
                    $"С доставкой: {service.PriceWithDelivery} руб. (По льготе {_discount}%: " +
                    $"{_actualPriceWithDelivery} руб.)";

                priceWithoutDilivery.Content = $"Без доставкой: {service.PriceWithoutDelivery} руб. (По льготе {_discount}%: " +
                    $"{_actualPriceWithoutDelivery} руб.)";
            }
            else
            {
                _actualPriceWithDelivery = service.PriceWithDelivery;
                _actualPriceWithoutDelivery = service.PriceWithoutDelivery;

                priceWithDilivery.Content = $"С доставкой: {service.PriceWithDelivery} руб.";
                priceWithoutDilivery.Content = $"Без доставкой: {service.PriceWithoutDelivery} руб.";
            }
            countSubscribers.Content = $"Подписчиков: {service.CountSubscribers}";

            if(benefits.Count > 0)
            {
                typeBenefits.Text = "";
                foreach (Data.BenefitService benefit in benefits)
                    typeBenefits.Text += benefit.NameBenefit + "\n";
            }
            else
            {
                typeBenefits.Text = "Нет";
            }
        }

        /// <summary>
        /// Найти максимальную скидку для пользователя
        /// </summary>
        /// <param name="benefits">Скидки сервиса</param>
        /// <returns>Максимальную скидку</returns>
        private long FindDiscount(List<Data.BenefitService> benefitsService)
        {
            if (benefitsService.Count == 0) return 0;

            long maxDiscount = 0;
            //Узнать есть ли льгота у пользователя
            List<Database.Models.UserBenefit> benefits = 
                Database.DBConnect.GetUserBenefit(Data.GlobalData.CurrentUser.id);

            if(benefits.Count > 0)
            {
                //Проход по льготам сервиса
                for(int i = 0; i < benefitsService.Count; i++)
                {
                    //Проход по льготам пользователя
                    for(int j = 0; j < benefits.Count; j++)
                    {
                        if(benefitsService[i].Data.IdBenefit == benefits[j].IdTypeBenfit)
                        {
                            if(benefitsService[i].Data.Discount > maxDiscount)
                            {
                                maxDiscount = benefitsService[i].Data.Discount;
                            }
                        }
                    }
                }
            }
            else
            {
                maxDiscount = 0;
            }

            return maxDiscount;
        }

        private long CalcDiscount(long value, long discount)
        {
            return (value / 100) * discount;
        }

        /// <summary>
        /// Создание диаграммы
        /// </summary>
        private void CreateDiagram()
        {
            List<Database.Models.DynamicService> dynamicServices = 
                Database.DBConnect.GetDynanicService(_currentService.Index);

            List<String> years = new List<string>();
            ChartValues<double> countSub = new ChartValues<double>();

            for(int i = 0; i < dynamicServices.Count; i++)
            {
                years.Add(dynamicServices[i].SubscriptionDateYear.ToString());
                countSub.Add(dynamicServices[i].CountSub);
            }

            //Прогнозирование на 2 года. Данные заносятся в dynamicServices
            ForecastTwoYear(dynamicServices);

            if (dynamicServices.Count > 2)
            {
                //Предпоследнее посчитанное значение
                countSub.Add(dynamicServices[dynamicServices.Count - 2].CountSub);
                //Последнее посчитанное значение
                countSub.Add(dynamicServices[dynamicServices.Count - 1].CountSub);

                years.Add(dynamicServices[dynamicServices.Count - 2].SubscriptionDateYear.ToString());
                years.Add(dynamicServices[dynamicServices.Count - 1].SubscriptionDateYear.ToString());
            }
            else
            {
                info.Content += " (недостаточно данных или нет данных)";
            }

            Axis axisX = new Axis();
            axisX.Title = "Года";
            axisX.Labels = years;
            DinamicChart.AxisX.Add(axisX);
            

            Axis axisY = new Axis();
            axisY.Title = "Подписчики";

            LiveCharts.Wpf.Separator sep = new LiveCharts.Wpf.Separator();
            sep.IsEnabled = false;
            sep.Step = 1;
            axisX.Separator = sep;

            DinamicChart.AxisY.Add(axisY);

            SeriesCollection sc = new SeriesCollection();
            
            LineSeries currentStats = new LineSeries() { Title = "Новых подписчиков", Values = countSub };
            currentStats.Stroke = Brushes.BlueViolet;

            sc.Add(currentStats);

            DinamicChart.Series = sc;

        }
        /// <summary>
        /// Прогнозирование на 2 года
        /// </summary>
        /// <param name="dynamicServices"></param>
        private void ForecastTwoYear(List<Database.Models.DynamicService> dynamicServices)
        {
            //Рассчет прогнозирования по формуле
            if (dynamicServices.Count > 0)
            {
                /* 
                 * b = СУМ((X-Xср)*(Y-Yср))/СУМ(X-Xср)^2
                 * формула ПРЕДСКАЗ из excel
                 */
                for (int i = 0; i < 2; i++)
                {
                    double dsXSred = 0.0; //x - средняя
                    double dsYSred = 0.0; //y - средняя
                    Database.Models.DynamicService ds = new Database.Models.DynamicService();
                    ds.SubscriptionDateYear = dynamicServices[dynamicServices.Count - 1].SubscriptionDateYear + 1;

                    //Подсчёт среднего
                    for (int j = 0; j < dynamicServices.Count; j++)
                    {
                        dsXSred += dynamicServices[j].SubscriptionDateYear;
                        dsXSred += dynamicServices[j].CountSub;
                    }

                    dsXSred /= dynamicServices.Count;
                    dsYSred /= dynamicServices.Count;

                    double sumUp = 0.0;
                    double sumDown = 0.0;
                    for (int j = 0; j < dynamicServices.Count; j++)
                    {
                        sumUp += (dynamicServices[i].SubscriptionDateYear - dsXSred) * (dynamicServices[i].CountSub - dsYSred);
                        sumDown += Math.Pow((dynamicServices[i].SubscriptionDateYear - dsXSred), 2);
                    }

                    //Приведение к понятным числам для прогноза (только положительные или 0)
                    int result = (int)(sumUp / sumDown);
                    long lastCount = dynamicServices[dynamicServices.Count - 1].CountSub - result;

                    if (lastCount < 0) lastCount = 0;

                    ds.CountSub = lastCount;

                    dynamicServices.Add(ds);
                }
            }
        }

        /// <summary>
        /// Подписка на сервис с доставкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubWithDelivery_Click(object sender, RoutedEventArgs e)
        {
            Database.Models.SubscriptionService subscriptionService = new Database.Models.SubscriptionService();
            subscriptionService.idUser = Data.GlobalData.CurrentUser.id;
            subscriptionService.idService = _currentIndex;
            subscriptionService.idTypeService = Database.DBConnect.GetTypeService(_currentIndex);
            subscriptionService.price = _currentService.PriceWithDelivery;
            subscriptionService.discount = _discount;
            subscriptionService.idSubscriptionType = 1; //ID в БД - с доставкой
            subscriptionService.finalPrice = _actualPriceWithDelivery;
            subscriptionService.term = int.Parse(termWithDelivery.Text);
            subscriptionService.subscriptionDate = DateTime.Now.ToString("d MMM yyyy");

            Sub(subscriptionService);
        }

        /// <summary>
        /// Подписка на сервис без доставкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubWithoutDelivery_Click(object sender, RoutedEventArgs e)
        {
            Database.Models.SubscriptionService subscriptionService = new Database.Models.SubscriptionService();
            subscriptionService.idUser = Data.GlobalData.CurrentUser.id;
            subscriptionService.idService = _currentIndex;
            subscriptionService.idTypeService = Database.DBConnect.GetTypeService(_currentIndex);
            subscriptionService.price = _currentService.PriceWithoutDelivery;
            subscriptionService.discount = _discount;
            subscriptionService.idSubscriptionType = 2; //ID в БД - без доставки
            subscriptionService.finalPrice = _actualPriceWithoutDelivery;
            subscriptionService.term = int.Parse(termWithoutDelivery.Text);
            subscriptionService.subscriptionDate = DateTime.Now.ToString("d MMM yyyy");

            Sub(subscriptionService);
        }

        private void Sub(Database.Models.SubscriptionService subscriptionService)
        {
            bool result = Database.DBConnect.AddSubscription(subscriptionService);
            if (result)
            {
                MessageBox.Show("Вы успешно подписались!");
                FillStructurePage();
            }
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
