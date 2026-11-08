using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace FantasyVolleyballLeague.VolleyballData.Database
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

            ConfigureUuidV7Keys(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Ids of <see cref="Entity"/> types are time-ordered UUID v7: the domain assigns them, so EF never generates
        /// them (new children added to a tracked aggregate are inserted, not updated), and rows inserted directly
        /// in SQL get a v7 id from PostgreSQL 18's <c>uuidv7()</c>.
        /// </summary>
        private static void ConfigureUuidV7Keys(ModelBuilder modelBuilder)
        {
            var idProperties = modelBuilder.Model.GetEntityTypes()
                .Where(entityType => typeof(Entity).IsAssignableFrom(entityType.ClrType))
                .Select(entityType => entityType.FindProperty(nameof(Entity.Id))!);

            foreach (var property in idProperties)
            {
                property.SetDefaultValueSql("uuidv7()");
                property.ValueGenerated = ValueGenerated.Never;
            }
        }
    }
}
