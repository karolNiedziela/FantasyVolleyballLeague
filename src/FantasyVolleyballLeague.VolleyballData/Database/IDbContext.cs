using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.VolleyballData.Database
{
    internal interface IDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
