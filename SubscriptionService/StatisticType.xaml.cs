using LiveCharts;
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
using System.Windows.Shapes;

namespace SubscriptionService
{
    /// <summary>
    /// Логика взаимодействия для StatisticType.xaml
    /// </summary>
    public partial class StatisticType : Window
    {
        public StatisticType()
        {
            InitializeComponent();
            ShowPieChartPopularTypes();
            ShowPieChartPopularService();
        }


        private void ShowPieChartPopularTypes()
        {
            List<Data.PieChartServiceType> values = Database.DBConnect.GetCountTypeServices();

            SeriesCollection collection = new SeriesCollection();

            for(int i = 0; i < values.Count; i++)
            {
                PieSeries pieSeries = new PieSeries
                {
                    Title = values[i].NameServiceType,
                    Values = new ChartValues<double> { values[i].Count },
                    DataLabels = true
                };

                collection.Add(pieSeries);
            }

            pieChartPopularTypes.Series = collection;

            pieChartPopularTypes.LegendLocation = LegendLocation.Right;
        }

        private void ShowPieChartPopularService()
        {
            List<Data.PopularServiceData> values = Database.DBConnect.GetPopularServiceData(5);

            SeriesCollection collection = new SeriesCollection();

            for (int i = 0; i < values.Count; i++)
            {
                PieSeries pieSeries = new PieSeries
                {
                    Title = values[i].NameService,
                    Values = new ChartValues<double> { values[i].SumCountSub },
                    DataLabels = true
                };

                collection.Add(pieSeries);
            }

            pieChartPopularService.Series = collection;

            pieChartPopularService.LegendLocation = LegendLocation.Bottom;
        }
    }
}
