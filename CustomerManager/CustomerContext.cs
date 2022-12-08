using CustomerManager.Model;
using Microsoft.EntityFrameworkCore;

namespace CustomerManager
{
    public class CustomerContext : DbContext
    {

        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();

    }
}