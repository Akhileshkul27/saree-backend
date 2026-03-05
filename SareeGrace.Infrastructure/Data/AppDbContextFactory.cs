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
            "Server=APAC-IND-LAP283\\SQLEXPRESS;Database=SareeGraceDB;Trusted_Connection=True;TrustServerCertificate=True;",
            b => b.MigrationsAssembly("SareeGrace.Infrastructure"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
