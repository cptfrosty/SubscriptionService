using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Data
{
    class UserSubscriptions
    {
        public long Index;
        public long IdUser;
        public string NameService;
        public string NameTypeService;
        public string NameSubscriptionType; //Название типа подписки
        public long Price;
        public long discount;
        public long FinalPrice;
        public string dateSub; //Дата подписки
        public long term; //Кол-во месяцев подписка
    }
}
