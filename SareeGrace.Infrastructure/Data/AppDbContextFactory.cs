using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SareeGrace.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by dotnet-ef tool when creating/applying migrations.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=tcp:sareegraceadmin.database.windows.net,1433;Initial Catalog=sareegrace-db;Persist Security Info=False;User ID=sareegraceadmin;Password=Akhilesh@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
            b => b.MigrationsAssembly("SareeGrace.Infrastructure"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
