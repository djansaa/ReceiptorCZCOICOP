using Microsoft.EntityFrameworkCore;

namespace ReceiptorCZCOICOP.Db
{

    /// <summary>
    /// Example of database context for the receipt database.
    /// </summary>
    internal class ReceiptDbContext : DbContext
    {
        public ReceiptDbContext(DbContextOptions<ReceiptDbContext> opts) : base(opts) { }

        public DbSet<ReceiptDbModel> Receipts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReceiptDbModel>(entity =>
            {
                entity.ToTable("Receipts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }

    /// <summary>
    /// Example of a model for the receipt database.
    /// </summary>
    internal class ReceiptDbModel
    {
        public int Id { get; set; }
        public string Company { get; set; } = "";
        public string Date { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Product { get; set; } = "";
        public decimal Price { get; set; }
        public string Coicop { get; set; } = "";
    }
}
