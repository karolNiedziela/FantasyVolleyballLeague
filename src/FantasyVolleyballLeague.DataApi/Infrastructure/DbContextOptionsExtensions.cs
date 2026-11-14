using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FantasyVolleyballLeague.DataApi.Infrastructure
{
    internal static class DbContextOptionsExtensions
    {
        // Without OrderBy the database returns rows in no defined order, so pages can repeat or skip rows.
        // Throwing makes a handler that forgets ApplySort fail in its first test.
        public static DbContextOptionsBuilder ThrowOnUnorderedPaging(this DbContextOptionsBuilder options)
        {
            return options.ConfigureWarnings(warnings => warnings.Throw(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
        }
    }
}
