using Microsoft.EntityFrameworkCore;

namespace FlightRadar.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<FlightPlan> FlightPlans { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Server> servers { get; set; }
    }
}
