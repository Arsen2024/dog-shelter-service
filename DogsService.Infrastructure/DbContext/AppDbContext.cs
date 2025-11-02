using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<DogEntity> Dogs { get; set; }
}