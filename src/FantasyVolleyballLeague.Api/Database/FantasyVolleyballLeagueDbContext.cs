using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace FantasyVolleyballLeague.Api.Database
{
    internal sealed class FantasyVolleyballLeagueDbContext : DbContext, IDbContext, IUnitOfWork
    {
        public FantasyVolleyballLeagueDbContext(DbContextOptions<FantasyVolleyballLeagueDbContext> options)
            : base(options)
        {
        }
        public new DbSet<TEntity> Set<TEntity>()
           where TEntity : class
           => base.Set<TEntity>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Database.BeginTransactionAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
