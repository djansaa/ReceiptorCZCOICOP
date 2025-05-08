using System.IO;

namespace ReceiptorCZCOICOP.Helpers
{
    /// <summary>
    /// Dictionary for COICOP codes.
    /// </summary>
    public static class CoicopDictionary
    {
        // dict word -> coicop
        private static readonly Dictionary<string, string> _dict;

        static CoicopDictionary()
        {
            _dict = new Dictionary<string, string>();

            // read coicop_dict.csv from Assets folder
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "coicop_dict.csv");

            if (!File.Exists(path)) return;

            // read all lines from the file and parse them into the dict
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("name;", StringComparison.OrdinalIgnoreCase)) continue;

                var parts = line.Split(';');
                if (parts.Length == 2)
                {
                    var name = parts[0].Trim();
                    var code = parts[1].Trim();
                    if (!_dict.ContainsKey(name))
                        _dict[name] = code;
                }
            }
        }

        /// <summary>
        /// Search for a COICOP code by name fragment.
        /// </summary>
        /// <param name="fragment">substring to be find in dictionary</param>
        /// <returns>list of names and their coicop</returns>
        public static IEnumerable<(string Name, string Coicop)> Search(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment)) return Enumerable.Empty<(string, string)>();

            // trim and convert to lowercase
            fragment = fragment.Trim().ToLowerInvariant();

            // filter entries that contain the fragment
            var matches = _dict.Where(e => e.Key.ToLowerInvariant().Contains(fragment));

            // select the best matches
            var ranked = matches
                .Select(e => new
                {
                    Entry = e,
                    Distance = Levenshtein(fragment, e.Key.ToLowerInvariant())
                })
                .OrderBy(r => r.Distance) // order by lev distance
                .ThenBy(r => r.Entry.Key.Length) // order by name length
                .Take(10) // take the first 10
                .Select(r => (r.Entry.Key, r.Entry.Value));

            return ranked;
        }

        /// <summary>
        /// Levenshtein distance algorithm.
        /// </summary>
        /// <param name="s">first string</param>
        /// <param name="t">second string</param>
        /// <returns>levenshtein distance between string s and string t</returns>
        private static int Levenshtein(string s, string t)
        {
            int n = s.Length, m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, // delete
                                 d[i, j - 1] + 1), // insert
                        d[i - 1, j - 1] + cost); // substitute
                }
            }
            return d[n, m];
        }
    }
}
