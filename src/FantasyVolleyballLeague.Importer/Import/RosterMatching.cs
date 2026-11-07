namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Finds a player of a roster from a statistics row that carries no player id (widget era).
    /// </summary>
    internal static class RosterMatching
    {
        /// <summary>
        /// Shirt number confirmed by name, then name. Members who belonged to the club on the match date are tried
        /// first, so a shirt number reused by a later signing, or a player who left, does not hide the right one.
        /// </summary>
        public static Guid? Find(IReadOnlyList<RosterMember> roster, int? shirtNumber, string nameKey, DateOnly? matchDate)
        {
            if (matchDate is not null)
            {
                var onTheDate = roster.Where(member => member.CoversDate(matchDate.Value)).ToList();
                var found = FindIn(onTheDate, shirtNumber, nameKey);
                if (found is not null)
                {
                    return found;
                }
            }

            return FindIn(roster, shirtNumber, nameKey);
        }

        private static Guid? FindIn(IReadOnlyList<RosterMember> members, int? shirtNumber, string nameKey)
        {
            var byNumber = shirtNumber is null ? [] : members.Where(member => member.ShirtNumber == shirtNumber).ToList();
            if (byNumber.Count == 1 && PlayerNameKey.ShareToken(byNumber[0].NameKey, nameKey))
            {
                return byNumber[0].PlayerId;
            }

            var byName = members.Where(member => member.NameKey == nameKey).ToList();

            return byName.Count == 1 ? byName[0].PlayerId : null;
        }
    }
}
