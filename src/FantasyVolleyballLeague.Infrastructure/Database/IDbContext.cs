using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.Infrastructure.Database
{
    internal interface IDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
