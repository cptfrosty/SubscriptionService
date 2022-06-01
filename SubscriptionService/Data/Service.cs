using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Data
{
    class Service
    {
        public long Index; //Индекс
        public string Name; //Название
        public string Type; //Тип (вид)
        public long PriceWithDelivery; //Стоимость с доставкой
        public long PriceWithoutDelivery; //Стоимость без доставки
        public bool IsBenefits; //Льготы
        public long CountSubscribers; //Кол-во подписчиков

    }
}
