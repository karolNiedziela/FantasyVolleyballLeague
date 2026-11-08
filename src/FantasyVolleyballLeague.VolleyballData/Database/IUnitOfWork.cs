using Microsoft.EntityFrameworkCore.Storage;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database
{
    internal interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
