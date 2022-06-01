using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Data
{
    class Search
    {
        public string Line; //Строка поиска

        //Критерии
        public string TypeService; //Тип сервиса
        public string TypeBenefit; //Тип льгот
        public string minPrice; //Минимальная цена
        public string maxPrice; //Максимальная цена
    }
}
