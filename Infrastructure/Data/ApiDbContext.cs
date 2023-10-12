using Microsoft.EntityFrameworkCore;
using PBL6.Domain.Models;

namespace PBL6.Infrastructure.Data
{
    public class ApiDbContext : DbContext
    {
        public DbSet<Example> Examples { get; set; }

        public ApiDbContext() { }

        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}