using Microsoft.EntityFrameworkCore;

namespace Anthology.Data.DB
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookFunnel> BookFunnelItems { get; set; }
        public DbSet<Classification> Classifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseLazyLoadingProxies().UseSqlite("Data Source=AppData/Config/Anthology.db");
#else
            optionsBuilder.UseLazyLoadingProxies().UseSqlite("Data Source=/config/Anthology.db");
#endif
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

    }
}
