using Microsoft.Extensions.FileSystemGlobbing;

namespace CompactGUI.Core;

public static class SkipListMatcher
{
    public static HashSet<string> GetExcludedFiles(string rootDirectory, IEnumerable<string> allFiles, IEnumerable<string> patterns)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!patterns.Any()) return excluded;

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (string glob in NormalisePatterns(patterns, rootDirectory))
        {
            matcher.AddInclude(glob);
        }

        var relativeFiles = allFiles.Select(f => Path.GetRelativePath(rootDirectory, f).Replace('\\', '/'));

        foreach (FilePatternMatch match in matcher.Match(relativeFiles).Files)
        {
            excluded.Add(Path.GetFullPath(Path.Combine(rootDirectory, match.Path)));
        }

        return excluded;
    }

    private static IEnumerable<string> NormalisePatterns(IEnumerable<string> entries, string rootDirectory)
    {
        foreach (string entry in entries)
        {
            string pattern = entry.Trim().Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (pattern.Length == 0) continue;

            // Absolute path entries (e.g. estimator full file names for non-Steam folders):
            // relativize against the root so they match the relative paths the matcher sees.
            if (Path.IsPathRooted(pattern))
            {
                try
                {
                    string relative = Path.GetRelativePath(rootDirectory, pattern).Replace('\\', '/');
                    if (!relative.StartsWith("../", StringComparison.Ordinal) && relative != "..")
                    {
                        pattern = relative;
                    }
                }
                catch (ArgumentException)
                {
                    // Unrelated roots; leave as-is (won't match).
                }
            }

            bool hasWildcard = pattern.Contains('*') || pattern.Contains('?');
            bool hasSeparator = pattern.Contains(Path.DirectorySeparatorChar) || pattern.Contains(Path.AltDirectorySeparatorChar);

            if (!hasSeparator && IsExtensionPattern(pattern))
            {
                // Extension patterns (".mp4", "*.mp4") match any file with that extension at any depth to keep existnig skips working.
                yield return "**/*" + (pattern.StartsWith('.') ? pattern : pattern[1..]);
            }
            else if (!hasWildcard && !hasSeparator)
            {
                // Legacy exact file names ("Thumbs.db"), matched at any depth.
                yield return "**/" + pattern;
            }
            else
            {
                yield return NormalisePath(pattern);
            }
        }
    }

    private static bool IsExtensionPattern(string pattern)
    {
        return pattern.StartsWith('.') || pattern.StartsWith("*.");
    }

    private static string NormalisePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
}
