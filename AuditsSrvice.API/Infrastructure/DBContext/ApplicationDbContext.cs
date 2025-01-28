using Microsoft.EntityFrameworkCore;
using SharedRepository.Audit;

namespace AuditSrvice.API.Infrastructure.DBContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Auditing> Audits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auditing>(entity =>
            {
                entity.ToTable("audit");
                entity.HasKey(e => e.AuditId); //Explicitly define the primary key

                entity.Property(e => e.AuditId).HasColumnName("audt_id")
                    .ValueGeneratedOnAdd(); // Configure for auto-increment

                entity.Property(e => e.ScreenName).HasColumnName("scr_nm");
                entity.Property(e => e.ObjectName).HasColumnName("obj_nm");
                entity.Property(e => e.ScreenPk).HasColumnName("scr_pk");
                entity.Property(e => e.AuditJson).HasColumnName("audt_json");
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
