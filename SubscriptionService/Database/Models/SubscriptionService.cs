using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Database.Models
{
    class SubscriptionService
    {
        public long idUser;
        public long idService;
        public long idTypeService;
        public long idSubscriptionType; //Тип подписки
        public long price;
        public long discount;
        public long finalPrice;
        public long term; //Срок подписки (по месяцам)
        public string subscriptionDate;
    }
}
