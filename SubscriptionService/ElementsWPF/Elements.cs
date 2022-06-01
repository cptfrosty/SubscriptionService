using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SubscriptionService.Data;

namespace SubscriptionService.ElementsWPF
{
    class Elements
    {
        /// <summary>
        /// Форма кнопки для отображения данных о подписках в личном кабинете
        /// </summary>
        /// <param name="subService">Информация</param>
        /// <param name="btnUnsub">Кнопка отписаться</param>
        /// <returns></returns>
        public static Border CreatePanelSubLK(UserSubscriptions subService, ref Button btnUnsub)
        {
            long index = subService.Index;
            string name = subService.NameService;
            string type = subService.NameTypeService;
            string typeSub = subService.NameSubscriptionType;
            long price = subService.Price;
            long finalPrice = subService.FinalPrice;
            long discount = subService.discount;
            string subDate = subService.dateSub;
            long term = subService.term;

            /*Элемент состоит из 6 полей: левое верхнее (1 поле), верхнее по середине (2 поле), 
             * правое верхнее (3 поле)
             * то же самое и снизу.
             * 1 поле - индекс подписки
             * 2 поле - название
             * 3 поле - вид (услуга, тип подписки)
             * 4 поле - цена (высчитаная с льготами)
             * 5 поле - пустое
             * 6 поле - кнопка "отписаться"
             */

            //BORDER
            Border elem = new Border();
            elem.BorderBrush = Brushes.Black;
            elem.BorderThickness = new System.Windows.Thickness(1);
            elem.Margin = new System.Windows.Thickness(0, 0, 5, 10);

            //GRID
            Grid grid = new Grid();
            grid.Height = 90;
            grid.Margin = new System.Windows.Thickness(5);
            RowDefinition row0 = new RowDefinition();
            RowDefinition row1 = new RowDefinition();
            ColumnDefinition column0 = new ColumnDefinition();
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();

            grid.RowDefinitions.Add(row0);
            grid.RowDefinitions.Add(row1);
            grid.ColumnDefinitions.Add(column0);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            //LABEL CONTENT INDEX
            Label indexInfo = new Label();
            indexInfo.Content = $"#E{index.ToString("00000")}";
            indexInfo.FontSize = 12;
            Grid.SetRow(indexInfo, 0);
            Grid.SetColumn(indexInfo, 0);
            indexInfo.FontWeight = System.Windows.FontWeights.Bold;
            indexInfo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            indexInfo.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            //TEXTBLOCK CONTENT NAME SUBSCRIBE
            TextBlock nameSubscribe = new TextBlock();
            nameSubscribe.Text = $"{name}";
            nameSubscribe.FontSize = 14;
            Grid.SetRow(nameSubscribe, 0);
            Grid.SetColumn(nameSubscribe, 1);
            nameSubscribe.FontWeight = System.Windows.FontWeights.Bold;
            nameSubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nameSubscribe.TextWrapping = TextWrapping.Wrap;

            //LABEL CONTENT TYPE SUBSCRIBE
            Label typeSubscribe = new Label();
            typeSubscribe.Content = $"{type}";
            typeSubscribe.FontSize = 12;
            Grid.SetRow(typeSubscribe, 0);
            Grid.SetColumn(typeSubscribe, 2);
            typeSubscribe.FontWeight = System.Windows.FontWeights.Bold;
            typeSubscribe.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            typeSubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            //LABEL CONTENT PRICE
            Label priceSubscribe = new Label();
            if(discount > 0)
            {
                priceSubscribe.Content = $"Цена по льготе: {finalPrice} руб. Скидка {discount}%. ({typeSub})";
            }
            else
            {
                priceSubscribe.Content = $"Цена: {price} руб. ({typeSub})";
            }
            priceSubscribe.FontSize = 12;
            Grid.SetRow(priceSubscribe, 1);
            Grid.SetColumn(priceSubscribe, 0);
            priceSubscribe.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            priceSubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            //TEXTBLOCK END SUBSCRIBE
            TextBlock endSubInfo = new TextBlock();

            //Расчет оставшегося времени
            DateTime dateSub = DateTime.Parse(subDate);
            DateTime endSub = new DateTime(
                dateSub.Year,
                dateSub.Month,
                dateSub.Day
            );
            
            endSub = endSub.AddMonths(int.Parse(term.ToString()));
            endSubInfo.Text = $"Подписка заканчивается : {endSub.ToString("dd.MM.yyyy")} г.\n";

            endSubInfo.FontSize = 14;
            Grid.SetRow(endSubInfo, 1);
            Grid.SetColumn(endSubInfo, 1);
            endSubInfo.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            endSubInfo.TextWrapping = TextWrapping.Wrap;
            endSubInfo.Margin = new Thickness(5);

            //LABEL BUTTON UNSUBSCRIBE
            Button unsubscribe = new Button();
            unsubscribe.Content = "Отписаться";
            unsubscribe.Name = $"E{index.ToString("00000")}";
            btnUnsub = unsubscribe;
            Grid.SetRow(unsubscribe, 1);
            Grid.SetColumn(unsubscribe, 2);
            unsubscribe.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            unsubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            grid.Children.Add(indexInfo);
            grid.Children.Add(nameSubscribe);
            grid.Children.Add(typeSubscribe);
            grid.Children.Add(priceSubscribe);
            grid.Children.Add(unsubscribe);
            grid.Children.Add(endSubInfo);

            elem.Child = grid;
            return elem;
        }

        /// <summary>
        /// Форма кнопки для отображения данных о поисках услуг
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Grid CreatePanelResultSearch(Service result)
        {
            //Распаковка класса
            long index = result.Index;
            string name = result.Name;
            string type = result.Type;
            long priceWithDelivery = result.PriceWithDelivery;
            long priceWithOutDelivery = result.PriceWithoutDelivery;
            bool isBenefits = result.IsBenefits;
            long countSubscribe = result.CountSubscribers;

            //Main Grid
            Grid grid = new Grid();
            grid.Margin = new System.Windows.Thickness(5);
            grid.Background = Brushes.AliceBlue;

            RowDefinition row1 = new RowDefinition();
            row1.Height = GridLength.Auto;
            RowDefinition row2 = new RowDefinition();
            row2.Height = GridLength.Auto;
            RowDefinition row3 = new RowDefinition();
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            ColumnDefinition column3 = new ColumnDefinition();

            grid.RowDefinitions.Add(row1);
            grid.RowDefinitions.Add(row2);
            grid.RowDefinitions.Add(row3);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.ColumnDefinitions.Add(column3);

            //LABEL CONTENT INDEX
            Label indexInfo = new Label();
            indexInfo.Content = $"#E{index.ToString("00000")}";
            indexInfo.FontSize = 12;
            Grid.SetRow(indexInfo, 0);
            Grid.SetColumn(indexInfo, 0);
            indexInfo.FontWeight = System.Windows.FontWeights.Bold;
            indexInfo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            indexInfo.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            //LABEL CONTENT NAME SUBSCRIBE
            TextBlock nameSubscribe = new TextBlock();
            nameSubscribe.Text = $"{name}";
            nameSubscribe.FontSize = 14;
            Grid.SetRow(nameSubscribe, 0);
            Grid.SetColumn(nameSubscribe, 1);
            nameSubscribe.FontWeight = System.Windows.FontWeights.Bold;
            nameSubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nameSubscribe.TextWrapping = TextWrapping.Wrap;

            //LABEL CONTENT TYPE SUBSCRIBE
            Label typeSubscribe = new Label();
            typeSubscribe.Content = $"{type}";
            typeSubscribe.FontSize = 12;
            Grid.SetRow(typeSubscribe, 0);
            Grid.SetColumn(typeSubscribe, 2);
            typeSubscribe.FontWeight = System.Windows.FontWeights.Bold;
            typeSubscribe.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            typeSubscribe.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            //LABEL COUNT SUBSCRIBE
            Label countSubscribeInfo = new Label();
            countSubscribeInfo.Content = $"Подписчиков: {countSubscribe}";
            Grid.SetRow(countSubscribeInfo, 1);
            Grid.SetColumn(countSubscribeInfo, 2);
            countSubscribeInfo.VerticalAlignment = VerticalAlignment.Center;
            countSubscribeInfo.HorizontalAlignment = HorizontalAlignment.Right;

            //STACKPANEL PRICE
            StackPanel priceSubscribe = new StackPanel();
            Grid.SetRow(priceSubscribe, 2);
            Grid.SetColumn(priceSubscribe, 0);
            priceSubscribe.HorizontalAlignment = HorizontalAlignment.Left;
            priceSubscribe.VerticalAlignment = VerticalAlignment.Bottom;
            priceSubscribe.Margin = new Thickness(0, 0, 0, 5);
            priceSubscribe.Orientation = Orientation.Horizontal;

            Label priceText = new Label();
            priceText.Content = "Цена: ";
            //Детали по цене
            StackPanel detailsPrise = new StackPanel();
            Label detailsPriceWithDelivery = new Label();
            detailsPriceWithDelivery.Content = $"С доставкой: {priceWithDelivery} руб.";
            Label detailsPriceWithOutDelivery = new Label();
            detailsPriceWithOutDelivery.Content = $"Без доставки: {priceWithOutDelivery} руб.";

            priceSubscribe.Children.Add(priceText);
            detailsPrise.Children.Add(detailsPriceWithDelivery);
            detailsPrise.Children.Add(detailsPriceWithOutDelivery);
            priceSubscribe.Children.Add(detailsPrise);

            //DISCOUNT ON THE BENEFIT 
            Label discountBenefit = new Label();
            if (isBenefits)
                discountBenefit.Content = "Скидка по льготам: да";
            else
                discountBenefit.Content = "Скидка по льготам: нет";
            Grid.SetRow(discountBenefit, 2);
            Grid.SetColumn(discountBenefit, 2);
            discountBenefit.HorizontalAlignment = HorizontalAlignment.Right;
            discountBenefit.VerticalAlignment = VerticalAlignment.Bottom;

            //ADD TO GRID
            grid.Children.Add(indexInfo);
            grid.Children.Add(nameSubscribe);
            grid.Children.Add(typeSubscribe);
            grid.Children.Add(priceSubscribe);
            grid.Children.Add(countSubscribeInfo);
            grid.Children.Add(discountBenefit);

            return grid;
        }
    }
}
