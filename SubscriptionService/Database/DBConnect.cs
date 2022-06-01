using SubscriptionService.Database.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace SubscriptionService.Database
{
    class DBConnect
    {
        static string pathDB = @".\Database\SubscriptionDB.db";
        static SQLiteConnection connection;
        static SQLiteCommand command;

        public static bool Connect()
        {
            try
            {
                connection = new SQLiteConnection("Data Source=" + pathDB + ";Version=3; FailIfMissing=False");
                connection.Open();
                return true;
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
                return false;
            }
        }

        //----COMMANDS---------------------//
        public static bool FindLoginAndPass(string login, string pass)
        {
            command = new SQLiteCommand(connection);

            command.CommandText = $"SELECT * FROM User WHERE login='{login}' and pass='{pass}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if (data.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                User user = new User();
                DataRow row = data.Rows[0];
                user.id = row.Field<long>("id");
                user.fio = row.Field<string>("FIO");
                user.idRole = row.Field<long>("idRole");
                

                Data.GlobalData.CurrentUser = user;
                return true;
            }
        }

        /// <summary>
        /// Перевод id роли в текст
        /// </summary>
        /// <param name="idRole">id роли</param>
        /// <returns>Название роли</returns>
        public static string GetRole(long idRole)
        {
            command = new SQLiteCommand(connection);
            command.CommandText = $"SELECT * FROM Role WHERE id='{idRole}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            return data.Rows[0].Field<string>("Name");
        }

        /// <summary>
        /// Получить льготы с названием
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns>Список льгот с переносом на новую строку</returns>
        public static string GetBenefitsName(long idUser)
        {
            string answer = "";
            command = new SQLiteCommand(connection);
            command.CommandText = 
                $"SELECT * FROM UserBenefit INNER JOIN TypeBenefit ON " +
                $"UserBenefit.idTypeBenefit = TypeBenefit.id WHERE UserBenefit.idUser='{idUser}';";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if (data.Rows.Count != 0)
            {
                foreach(DataRow row in data.Rows)
                {
                    answer += row.Field<string>("NameBenefit") + "\n";
                }
            }
            else {
                answer = "нет";
            }

            return answer;
        }

        //TODO: ОЧЕНЬ НАГРУЖЕННЫЙ МЕТОД. РАЗГРУЗИТЬ
        public static List<Data.Service> GetSearchResult(Data.Search search)
        {
            //Распаковка класса
            string lineSearch = search.Line;
            string typeService = search.TypeService;
            string typeBenefit = search.TypeBenefit;
            string maxPrice = search.maxPrice;
            string minPrice = search.minPrice;

            List<Data.Service> resultSearch = new List<Data.Service>();

            command = new SQLiteCommand(connection);

            if (typeBenefit == "Все")
            {
                command.CommandText =
                        $"SELECT Service.id, Service.NameService, TypeService.NameTypeService, " +
                        $"Service.priceWithDelivery, Service.priceWithoutDelivery, Service.countSubscribers"
                        + $" FROM (Service INNER JOIN TypeService ON Service.idTypeService = TypeService.id) ";
            }
            else
            {
                //Поиск по льготам
                command.CommandText = $"SELECT Service.id, Service.NameService, TypeService.NameTypeService," +
                    $" Service.priceWithDelivery, Service.priceWithoutDelivery, TypeBenefit.NameBenefit, " +
                    $" Service.countSubscribers, BenefitService.discount"
                    +" FROM(BenefitService INNER JOIN Service, TypeBenefit, TypeService ON"
                    +" BenefitService.idService = Service.id AND"
                    +" BenefitService.idTypeBenefit = TypeBenefit.id AND"
                    +" Service.idTypeService = TypeService.id) ";
            }
            
            //Формирование запроса
            if(!string.IsNullOrEmpty(lineSearch) || typeService != "Все" || typeBenefit != "Все"
                || !string.IsNullOrEmpty(maxPrice) || !string.IsNullOrEmpty(minPrice))
            {
                bool isFirstRequest = false; //Присутствие первого запроса
                command.CommandText += "WHERE ";
                //Если в поиске было что-то указано, то отобразить все возможные подписки
                if (!string.IsNullOrEmpty(lineSearch))
                {
                    command.CommandText += $"Service.NameService like '%{lineSearch}%'";
                    isFirstRequest = true;
                }

                if(typeService != "Все")
                {
                    if (isFirstRequest) command.CommandText += " AND ";
                    command.CommandText += $"TypeService.NameTypeService = '{typeService}'";
                    isFirstRequest = true;
                }

                if(typeBenefit != "Все")
                {
                    if (isFirstRequest) command.CommandText += " AND ";
                    command.CommandText += $"TypeBenefit.NameBenefit = '{typeBenefit}'";
                    isFirstRequest = true;
                }

                if (!string.IsNullOrEmpty(minPrice))
                {
                    if (isFirstRequest) command.CommandText += " AND ";
                    command.CommandText += $"Service.priceWithoutDelivery >= '{minPrice}'";
                    isFirstRequest = true;
                }

                if (!string.IsNullOrEmpty(maxPrice))
                {
                    if (isFirstRequest) command.CommandText += " AND ";
                    command.CommandText += $"Service.priceWithDelivery <= '{maxPrice}'";
                }
            }
            
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);


            foreach (DataRow row in data.Rows)
            {
                Data.Service service = new Data.Service();
                service.Index = row.Field<long>("id");
                service.Name = row.Field<string>("NameService");
                service.Type = row.Field<string>("NameTypeService");
                service.PriceWithDelivery = row.Field<long>("priceWithDelivery");
                service.PriceWithoutDelivery = row.Field<long>("priceWithoutDelivery");
                service.CountSubscribers = row.Field<long>("countSubscribers");
                service.IsBenefits = GetAvailabilityBenefits(service.Index);

                resultSearch.Add(service);
            }

            return resultSearch;
        }

        /// <summary>
        /// Получить информацию о сервисе
        /// </summary>
        /// <param name="index">Индекс сервиса</param>
        /// <param name="typeBenefit">Возвращает льготы по сервису</param>
        /// <returns>Информацию о сервисе</returns>
        public static Data.Service GetService(long index, ref List<Data.BenefitService> typeBenefit)
        {
            Data.Service service = new Data.Service();
            command = new SQLiteCommand(connection);
            command.CommandText =
                        $"SELECT Service.id, Service.NameService, TypeService.NameTypeService, " +
                        $"Service.priceWithDelivery, Service.priceWithoutDelivery, Service.countSubscribers" +
                        $" FROM (Service INNER JOIN TypeService ON Service.idTypeService = TypeService.id) " +
                        $"WHERE Service.id = '{index}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            service.Index = data.Rows[0].Field<long>("id");
            service.Name = data.Rows[0].Field<string>("NameService");
            service.Type = data.Rows[0].Field<string>("NameTypeService");
            service.PriceWithDelivery = data.Rows[0].Field<long>("priceWithDelivery");
            service.PriceWithoutDelivery = data.Rows[0].Field<long>("priceWithoutDelivery");
            service.CountSubscribers = data.Rows[0].Field<long>("countSubscribers");
            service.IsBenefits = GetAvailabilityBenefits(service.Index);

            typeBenefit = GetBenefitService(index);

            return service;
        }


        /// <summary>
        /// Получить все типы сервисов
        /// </summary>
        /// <returns>Типы сервисов</returns>
        public static List<string> GetTypeServices()
        {
            List<string> result = new List<string>();
            command = new SQLiteCommand(connection);

            command.CommandText = $"SELECT TypeService.NameTypeService FROM TypeService";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach(DataRow row in data.Rows)
            {
                result.Add(row.Field<string>("NameTypeService"));
            }

            return result;
        }

        public static List<string> GetTypeBenefit()
        {
            List<string> result = new List<string>();
            command = new SQLiteCommand(connection);

            command.CommandText = $"SELECT TypeBenefit.NameBenefit FROM TypeBenefit";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach (DataRow row in data.Rows)
            {
                result.Add(row.Field<string>("NameBenefit"));
            }

            return result;
        }

        /// <summary>
        /// Получить информацию о наличии льгот в сервисе
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<Data.BenefitService> GetBenefitService(long index)
        {
            List<Data.BenefitService> result = new List<Data.BenefitService>();
            command = new SQLiteCommand(connection);

            command.CommandText = $"SELECT BenefitService.id, BenefitService.idService, " +
                $"BenefitService.idTypeBenefit, BenefitService.discount, " +
                $"TypeBenefit.NameBenefit, Service.NameService " +
                $"FROM(BenefitService INNER JOIN TypeBenefit, Service ON " +
                $"BenefitService.id = TypeBenefit.id and BenefitService.idService = Service.id) " +
                $"WHERE BenefitService.idService = '{index}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach (DataRow row in data.Rows)
            {
                //Модель данных базы данных
                Database.Models.BenefitService bs = new BenefitService();
                bs.Id = row.Field<long>("id");
                bs.IdService = row.Field<long>("idService");
                bs.IdBenefit = row.Field<long>("idTypeBenefit");
                bs.Discount = row.Field<long>("discount");

                //Модель данных программы с содержанием модели данных базы данных
                Data.BenefitService benefitService = new Data.BenefitService();
                benefitService.Data = bs;
                benefitService.NameService = row.Field<string>("NameService");
                benefitService.NameBenefit = row.Field<string>("NameBenefit");

                result.Add(benefitService);
            }

            return result;
        }

        public static List<UserBenefit> GetUserBenefit(long idUser)
        {
            List<UserBenefit> result = new List<UserBenefit>();
            command.CommandText = $"SELECT  * FROM UserBenefit WHERE UserBenefit.idUser = '{idUser}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach(DataRow row in data.Rows)
            {
                UserBenefit userBenefit = new UserBenefit();
                userBenefit.Id = row.Field<long>("Id");
                userBenefit.IdUser = row.Field<long>("idUser");
                userBenefit.IdTypeBenfit = row.Field<long>("idTypeBenefit");

                result.Add(userBenefit);
            }

            return result;
        } 

        public static long GetTypeService(long idService)
        {
            command.CommandText = $"SELECT * FROM Service WHERE Service.id = '{idService}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            return data.Rows[0].Field<long>("idTypeService");
        }

        public static bool AddSubscription(Models.SubscriptionService subscription)
        {
            long idUser             = subscription.idUser;
            long idService          = subscription.idService;
            long idTypeService      = subscription.idTypeService;
            long idSubType          = subscription.idSubscriptionType;
            long price              = subscription.price;
            long discount           = subscription.discount;
            long finalPrice         = subscription.finalPrice;
            long term               = subscription.term;
            string subscriptionDate = subscription.subscriptionDate;

            command = new SQLiteCommand(connection);
            command.CommandText =
                $"INSERT INTO SubscriptionService (idUser, idService, idTypeService, idSubscriptionType, " +
                $"price, discount, finalPrice, term, subscriptionDate) " +
                $"VALUES('{idUser}', '{idService}', '{idTypeService}', '{idSubType}', " +
                $"'{price}', '{discount}', '{finalPrice}', '{term}', '{subscriptionDate}')";
            
            command.ExecuteNonQueryAsync();
            

            command.CommandText = $"UPDATE Service SET countSubscribers = countSubscribers + 1 WHERE Service.id = {idService}";
            command.ExecuteNonQueryAsync();

            //Добавление статистики для подписки
            DateTime dt = DateTime.Parse(subscriptionDate);
            AddDynamicService(idService, dt.Year);
           
            return true;
        }

        /// <summary>
        /// Получить информацию о подписки пользователя
        /// </summary>
        /// <param name="idUser">ID пользователя</param>
        /// <returns></returns>
        public static List<Data.UserSubscriptions> GetSubscriptionServiceUser(long idUser)
        {
            List<Data.UserSubscriptions> result = new List<Data.UserSubscriptions>();

            command = new SQLiteCommand(connection);
            command.CommandText =
                $"SELECT SubscriptionService.idService, SubscriptionService.idUser, " +
                $"Service.NameService, TypeService.NameTypeService, SubscriptionType.nameSubscriptionType, " +
                $"SubscriptionService.price, SubscriptionService.discount, SubscriptionService.finalPrice, " +
                $"SubscriptionService.subscriptionDate, SubscriptionService.term " +
                $"FROM SubscriptionService INNER JOIN Service, TypeService, SubscriptionType " +
                $"ON SubscriptionService.idService = Service.id "+
                $"AND SubscriptionService.idTypeService = TypeService.id "+
                $"AND SubscriptionService.idSubscriptionType = SubscriptionType.id "+
                $"WHERE SubscriptionService.idUser = '{idUser}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach (DataRow row in data.Rows)
            {
                Data.UserSubscriptions subService = new Data.UserSubscriptions();
                subService.Index = row.Field<long>("idService");
                subService.IdUser = row.Field<long>("idUser");
                subService.NameService = row.Field<string>("NameService");
                subService.NameTypeService = row.Field<string>("NameTypeService");
                subService.NameSubscriptionType = row.Field<string>("nameSubscriptionType");
                subService.Price = row.Field<long>("price");
                subService.discount = row.Field<long>("discount");
                subService.FinalPrice = row.Field<long>("finalPrice");
                subService.dateSub = row.Field<string>("subscriptionDate");
                subService.term = row.Field<long>("term");

                result.Add(subService);
            }

            return result;
        }

        public static bool CheckSubService(long idUser, long idService)
        {
            command.CommandText = $"SELECT * FROM SubscriptionService " +
                $"WHERE SubscriptionService.idUser = '{idUser}' AND SubscriptionService.idService = '{idService}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if (data.Rows.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Проверить наличие льгот у сервиса
        /// </summary>
        /// <returns>Результат наличия льгот</returns>
        private static bool GetAvailabilityBenefits(long index)
        {
            bool result = false;
            command = new SQLiteCommand(connection);
            command.CommandText = $"SELECT COUNT(*) FROM BenefitService where BenefitService.idService = '{index}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if(data.Rows[0].Field<long>("COUNT(*)") > 0)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Отписаться от сервиса
        /// </summary>
        /// <param name="idUser">ID пользователя</param>
        /// <param name="idService">ID сервиса</param>
        public static void UnsubscribeService(long idUser, long idService)
        {
             command = new SQLiteCommand(connection);
             command.CommandText =
                 $"DELETE FROM SubscriptionService WHERE SubscriptionService.idUser = '{idUser}' " +
                 $"AND SubscriptionService.idService = '{idService}'";

             command.ExecuteNonQueryAsync();


             command.CommandText = $"UPDATE Service SET countSubscribers = countSubscribers - 1 WHERE Service.id = {idService}";
             command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Проверить роль на администратора
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        public static bool CheckRoleAdministration(long idUser)
        {
            bool result = false;
            command = new SQLiteCommand(connection);
            command.CommandText = $"SELECT User.id, Role.Name " +
                $"FROM User INNER JOIN Role ON User.idRole = Role.id WHERE User.id = '{idUser}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if (data.Rows[0].Field<string>("Name") == "Администратор")
            {
                result = true;
            }

            return result;
        }

        private static void AddDynamicService(long idService, long year)
        {
            command = new SQLiteCommand(connection);

            //Проверка наличия строки
            command.CommandText = $"SELECT * FROM DynamicsService WHERE DynamicsService.idService = '{idService}'" +
                $"AND DynamicsService.subscriptionDateYear = '{year}'";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            if (data.Rows.Count == 0)
            {
                //Если данной строки нет, то создать
                command.CommandText =
                    $"INSERT INTO DynamicsService(idService, subscriptionDateYear, countSub) " +
                    $"VALUES('{idService}', '{year}', '0')";
                command.ExecuteNonQueryAsync();
            }

            command.CommandText = $"UPDATE DynamicsService SET countSub = countSub + 1 WHERE DynamicsService.idService = '{idService}' " +
                $"AND DynamicsService.subscriptionDateYear = '{year}'";
            
            command.ExecuteNonQueryAsync();
        }

        public static List<Database.Models.DynamicService> GetDynanicService(long idService)
        {
            List<Database.Models.DynamicService> res = new List<DynamicService>();

            command.CommandText = $"SELECT * FROM DynamicsService WHERE DynamicsService.idService = '{idService}'";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach (DataRow row in data.Rows)
            {
                Database.Models.DynamicService dynamicService = new DynamicService();
                dynamicService.Id = row.Field<long>("id");
                dynamicService.IdService = row.Field<long>("idService");
                dynamicService.SubscriptionDateYear = row.Field<long>("subscriptionDateYear");
                dynamicService.CountSub = row.Field<long>("countSub");

                res.Add(dynamicService);
            }

            return res;
        }

        public static List<Data.PieChartServiceType> GetCountTypeServices()
        {
            List<Data.PieChartServiceType> res = new List<Data.PieChartServiceType>();

            command.CommandText = $"SELECT * FROM TypeService";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach(DataRow row in data.Rows)
            {
                Data.PieChartServiceType pieChartServiceType = new Data.PieChartServiceType();

                long id = row.Field<long>("id");
                string nameType = row.Field<string>("nameTypeService");

                command.CommandText = $"SELECT COUNT(*) FROM Service WHERE Service.idTypeService = '{id}'";
                DataTable resCount = new DataTable();
                adapter = new SQLiteDataAdapter(command);
                adapter.Fill(resCount);

                pieChartServiceType.NameServiceType = nameType;
                pieChartServiceType.Count = (int)resCount.Rows[0].Field<long>("COUNT(*)");
                res.Add(pieChartServiceType);
            }

            return res;
        }

        public static List<Data.PopularServiceData> GetPopularServiceData(int limit)
        {
            List<Data.PopularServiceData> result = new List<Data.PopularServiceData>();

            command.CommandText = $"SELECT DynamicsService.id, Service.NameService, SUM(DynamicsService.countSub) "+
            $"AS countSub FROM DynamicsService INNER JOIN Service ON DynamicsService.idService = Service.id " +
            $"GROUP BY DynamicsService.idService ORDER BY DynamicsService.countSub DESC LIMIT {limit}";

            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);

            foreach(DataRow row in data.Rows)
            {
                Data.PopularServiceData popularServiceData = new Data.PopularServiceData();
                popularServiceData.NameService = row.Field<string>("NameService");
                popularServiceData.SumCountSub = row.Field<long>("countSub");
                
                result.Add(popularServiceData);
            }

            return result;
        }
    }
}
