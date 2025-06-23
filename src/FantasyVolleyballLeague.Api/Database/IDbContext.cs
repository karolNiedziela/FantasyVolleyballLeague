using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.Api.Database
{
    internal interface IDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
