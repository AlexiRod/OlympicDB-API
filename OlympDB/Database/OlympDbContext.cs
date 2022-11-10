using Microsoft.EntityFrameworkCore;
using OlympDB.Classes;

namespace OlympDB.Database
{
    public class OlympDbContext : DbContext
    {
        public OlympDbContext()
        {
            Database.EnsureCreated();
        }

        public OlympDbContext(DbContextOptions<OlympDbContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=olymp;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Result>().HasKey(r => new { r.EventId, r.PlayerId });

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Olympiс> Olympics { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Result> Results { get; set; }
    }

}
