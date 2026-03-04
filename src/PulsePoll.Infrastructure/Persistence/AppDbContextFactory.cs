using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PulsePoll.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=winpc;Port=5432;Database=pulsepoll_db;Username=postgres;Password=Dev123456!")
                      .UseSnakeCaseNamingConvention();
        return new AppDbContext(optionsBuilder.Options);
    }
}
