using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfCoreDemo.Infrastructure.Persistence;

/// <summary>
/// Usada pelas ferramentas <c>dotnet ef</c> (migrations) em tempo de design,
/// quando a aplicação não está em execução.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=EfCoreDemo;Trusted_Connection=True;")
            .Options;

        return new AppDbContext(options);
    }
}
