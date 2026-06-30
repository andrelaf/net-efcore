using EfCoreDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EfCoreDemo.Infrastructure.Persistence;

/// <summary>
/// DbContext principal da demonstração. As configurações de mapeamento ficam em
/// classes <c>IEntityTypeConfiguration</c> separadas (pasta Configurations) e são
/// aplicadas via varredura do assembly.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<PhysicalBook> PhysicalBooks => Set<PhysicalBook>();
    public DbSet<EBook> EBooks => Set<EBook>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as IEntityTypeConfiguration<T> deste assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
