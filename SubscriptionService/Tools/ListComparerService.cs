using SubscriptionService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Tools
{
    class ListComparerServiceCountSub : IComparer<Data.Service>
    {
        public int Compare(Service x, Service y)
        {
            if (x.CountSubscribers > y.CountSubscribers)
            {
                return -1;
            }
            else if(x.CountSubscribers < y.CountSubscribers)
            {
                return 1;
            }
            return 0;
        }
    }
}
