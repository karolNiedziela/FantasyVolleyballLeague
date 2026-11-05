namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class StorageOptions
    {
        public const string SectionName = "Storage";

        private const string SolutionFileName = "FantasyVolleyballLeague.sln";

        /// <summary>
        /// Root folder of the scraped data. A relative path is resolved against the solution directory.
        /// </summary>
        public string DataRoot { get; set; } = "data";

        public string ResolveDataRoot()
        {
            if (Path.IsPathRooted(DataRoot))
            {
                return DataRoot;
            }

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return Path.GetFullPath(Path.Combine(directory.FullName, DataRoot));
                }

                directory = directory.Parent;
            }

            return Path.GetFullPath(DataRoot);
        }
    }
}
