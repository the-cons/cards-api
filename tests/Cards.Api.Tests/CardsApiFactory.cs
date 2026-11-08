using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Cards.Api.Tests;

public class CardsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = 
        Path.Combine(Path.GetTempPath(), $"cards-test-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Cards", $"Data Source={_dbPath}");
        builder.UseSetting("Cards:MaxCollectionSize", "5");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }
}