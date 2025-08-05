using Microsoft.EntityFrameworkCore;
using ReceiptorCZCOICOP.Db;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ReceiptDbContextTests
{
    [Fact]
    public async Task CanAddAndRetrieveReceiptDbModel()
    {
        // arrange
        var options = new DbContextOptionsBuilder<ReceiptDbContext>()
            .UseInMemoryDatabase(databaseName: "testdb")
            .Options;

        using var context = new ReceiptDbContext(options);
        var receipt = new ReceiptDbModel
        {
            Company = "Test Company",
            Date = "2023-10-01",
            Currency = "USD",
            Product = "Test Product",
            Price = 10.5m,
            Coicop = "123"
        };

        // add receipt
        context.Receipts.Add(receipt);
        await context.SaveChangesAsync();
        var retrieved = context.Receipts.FirstOrDefault();

        // asserts
        Assert.NotNull(retrieved);
        Assert.Equal("Test Company", retrieved.Company);
        Assert.Equal("Test Product", retrieved.Product);
        Assert.Equal(10.5m, retrieved.Price);
    }
}