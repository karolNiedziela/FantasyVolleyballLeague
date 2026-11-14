using FantasyVolleyballLeague.VolleyballData.Domain;
using System.Linq.Expressions;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal sealed class SortMap<T> where T : Entity
    {
        private readonly Dictionary<string, ISortKey<T>> _sortKeys = new(StringComparer.OrdinalIgnoreCase);

        private string? _defaultSort;

        public IReadOnlyCollection<string> Fields => _sortKeys.Keys;

        public string? DefaultSort => _defaultSort;

        public SortMap<T> Add<TKey>(string key, Expression<Func<T, TKey>> keySelector)
        {
            _sortKeys[key] = new TypedSortKey<T, TKey>(keySelector);

            return this;
        }

        public SortMap<T> WithDefault(string sort)
        {
            var parsed = ParseFields(sort);
            if (!parsed.IsValid)
            {
                var messages = string.Join(" ", parsed.Errors.Values.SelectMany(fieldErrors => fieldErrors));

                throw new ArgumentException($"Invalid default sort '{sort}'. {messages}", nameof(sort));
            }

            _defaultSort = sort;

            return this;
        }

        public SortParseResult<T> Parse(string? sortQuery)
        {
            var query = string.IsNullOrWhiteSpace(sortQuery) ? _defaultSort : sortQuery;
            if (query is null)
            {
                return SortParseResult<T>.Success([]);
            }

            return ParseFields(query);
        }

        private SortParseResult<T> ParseFields(string sort)
        {
            var errors = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var instructions = new List<SortInstruction<T>>();

            var tokens = sort.Split(',', StringSplitOptions.TrimEntries);
            foreach (var token in tokens)
            {
                var ascending = !token.StartsWith('-');
                var name = ascending ? token : token[1..];

                if (name.Length == 0)
                {
                    errors.Add("Sort fields must not be empty.");
                }
                else if (!_sortKeys.TryGetValue(name, out var sortKey))
                {
                    errors.Add($"Unknown sort field '{name}'. Allowed fields: {string.Join(", ", _sortKeys.Keys)}.");
                }
                else if (!seen.Add(name))
                {
                    errors.Add($"Sort field '{name}' is listed more than once.");
                }
                else
                {
                    instructions.Add(new SortInstruction<T>(sortKey, ascending));
                }
            }

            if (errors.Count > 0)
            {
                return SortParseResult<T>.Failure(SortingQueryParameters.Sort, errors);
            }

            return SortParseResult<T>.Success(instructions);
        }
    }
}
