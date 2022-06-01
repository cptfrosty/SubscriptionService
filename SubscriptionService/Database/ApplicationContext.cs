using System.Data.Entity;
using SubscriptionService.Database.Models;

namespace SubscriptionService.Database
{
    class ApplicationContext: DbContext
    {
        public ApplicationContext() : base("DefaultConnection")
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
