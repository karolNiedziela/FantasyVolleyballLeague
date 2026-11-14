using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal sealed class SortParseResult<T> where T : Entity
    {
        private readonly IReadOnlyList<SortInstruction<T>> _instructions;

        private SortParseResult(IReadOnlyList<SortInstruction<T>> instructions, IReadOnlyDictionary<string, string[]> errors)
        {
            _instructions = instructions;
            Errors = errors;
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public bool IsValid => Errors.Count == 0;

        public static SortParseResult<T> Success(IReadOnlyList<SortInstruction<T>> instructions)
        {
            return new SortParseResult<T>(instructions, new Dictionary<string, string[]>());
        }

        public static SortParseResult<T> Failure(string parameter, IEnumerable<string> messages)
        {
            return new SortParseResult<T>([], new Dictionary<string, string[]> { [parameter] = messages.ToArray() });
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("The sort query is invalid. Check IsValid before applying it.");
            }

            IOrderedQueryable<T>? ordered = null;
            foreach (var instruction in _instructions)
            {
                ordered = ordered is null
                    ? instruction.Key.OrderBy(query, instruction.Ascending)
                    : instruction.Key.ThenBy(ordered, instruction.Ascending);
            }

            return ordered is null ? query.OrderBy(entity => entity.Id) : ordered.ThenBy(entity => entity.Id);
        }
    }
}
