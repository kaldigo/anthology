using Microsoft.EntityFrameworkCore;

namespace Anthology.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Classification> Classifications { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlite("Data Source=" + Path.Combine(Utils.FileUtils.GetConfigPath(),"Anthology.db"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

    }
}
