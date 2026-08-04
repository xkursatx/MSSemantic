using Microsoft.EntityFrameworkCore;
using MSSemantic.Models;

namespace MSSemantic.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        public DbSet<ProductModel> Products => Set<ProductModel>();
        public DbSet<SessionModel> Sessions => Set<SessionModel>();
        public DbSet<MessageModel> Messages => Set<MessageModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("products");

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
                entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(x => x.Detail).HasColumnName("detail");
                entity.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
                entity.Property(x => x.InStock).HasColumnName("in_stock");

                // Seed data
                entity.HasData(
                    new ProductModel
                    {
                        Id = 1,
                        Name = "Masa Lambası",
                        Description = "Masaüstünüzde şık ve modern bir aydınlatma çözümü",
                        Detail = "Masa lambası, çalışma alanınızı aydınlatmak için tasarlanmış şık ve modern bir aydınlatma çözümüdür. Ayarlanabilir ışık seviyesi ve enerji tasarruflu LED teknolojisi ile kullanıcı dostu bir deneyim sunar. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 25x30x80 cm ebatlarındadır.",
                        Price = 300,
                        InStock = 21
                    },
                    new ProductModel
                    {
                        Id = 2,
                        Name = "Tavan Aydınlatması",
                        Description = "Dış mekan veranda ışığı",
                        Detail = "Tavan aydınlatması, dış mekan veranda ışığı olarak tasarlanmıştır. Suya dayanıklı malzemelerden üretilmiştir ve enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 50x50x20 cm ebatlarındadır.",
                        Price = 500,
                        InStock = 0
                    },
                    new ProductModel
                    {
                        Id = 3,
                        Name = "Avize",
                        Description = "Şık ve modern bir avize",
                        Detail = "Avize, şık ve modern bir tasarıma sahip olup, yaşam alanınıza estetik bir dokunuş katar. Enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 60x60x40 cm ebatlarındadır.",
                        Price = 800,
                        InStock = 35
                    }
                );
            });

            modelBuilder.Entity<SessionModel>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("sessions");

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasMany(x => x.Messages)
                    .WithOne(x => x.Session)
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MessageModel>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("messages");

                entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(x => x.SessionId).HasColumnName("session_id");
                entity.Property(x => x.Role).HasColumnName("role").IsRequired();
                entity.Property(x => x.Content).HasColumnName("content").IsRequired();
                entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasIndex(x => x.SessionId).HasDatabaseName("idx_messages_session_id");
            });
        }
    }
}