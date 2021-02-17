using Microsoft.EntityFrameworkCore;

namespace MZPO.DBRepository
{
    public class MySQLContext : DbContext
    {
        public DbSet<AmoAccountAuth> AmoAccounts { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CF> CFs { get; set; }

        public MySQLContext(DbContextOptions<MySQLContext> opt) : base(opt)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AmoAccountAuth>().HasKey(x => x.id);
            modelBuilder.Entity<AmoAccountAuth>().Property(x => x.id).ValueGeneratedNever();
            modelBuilder.Entity<AmoAccountAuth>().Property(x => x.name).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<AmoAccountAuth>().Property(x => x.subdomain).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<AmoAccountAuth>().Property(x => x.client_id).IsRequired();
            modelBuilder.Entity<AmoAccountAuth>().Property(x => x.client_secret).IsRequired();

            modelBuilder.Entity<City>().HasKey(x => x.EngName);
            modelBuilder.Entity<City>().Property(x => x.EngName).HasMaxLength(255);
            modelBuilder.Entity<City>().Property(x => x.RusName).IsRequired().HasMaxLength(255);

            modelBuilder.Entity<Tag>().HasKey(x => new { x.Id, x.AmoId });
            modelBuilder.Entity<Tag>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Tag>().Property(x => x.Name).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<Tag>().Property(x => x.EntityName).HasMaxLength(255);

            modelBuilder.Entity<CF>().HasKey(x => new { x.Id, x.AmoId });
            modelBuilder.Entity<CF>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<CF>().Property(x => x.Name).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<CF>().Property(x => x.EntityName).HasMaxLength(255);
        }
    }
}
