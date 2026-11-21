namespace FantasyVolleyballLeague.VolleyballData.Database
{
    internal interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the operation in one transaction, inside the context's execution strategy. After a transient failure the
        /// strategy runs the whole operation again in a new transaction, so the operation must be safe to repeat: it must
        /// not depend on state it changed outside the database. The transaction is committed when the operation completes
        /// and rolled back when it throws.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    }
}
