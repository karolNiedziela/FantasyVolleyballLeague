using Microsoft.EntityFrameworkCore.Storage;

namespace FantasyVolleyballLeague.Infrastructure.Database
{
    internal interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
