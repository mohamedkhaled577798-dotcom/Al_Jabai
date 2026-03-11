using Microsoft.EntityFrameworkCore;

namespace AlJabai.Infrastructure.Data
{
    public class AlJabaiDbContext : DbContext
    {
        public AlJabaiDbContext(DbContextOptions<AlJabaiDbContext> options)
            : base(options)
        {
        }

        // Add AlJabai-specific entities here
        // public DbSet<TaxRecord> TaxRecords { get; set; }
    }
}
