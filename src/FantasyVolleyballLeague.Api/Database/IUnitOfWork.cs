using Microsoft.EntityFrameworkCore.Storage;

namespace FantasyVolleyballLeague.Api.Database
{
    internal interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
