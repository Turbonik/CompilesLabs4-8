using System.Text.RegularExpressions;

public static class RegexLibrary
{
    public const string CloseP = @"</[pP]>";
    public const string SnakeCase = @"\b[a-z]+(?:_[a-z]+)+\b";
    public const string DOI = @"\b(?:https?://(?:dx\.)?doi\.org/|doi:)?10\.\d{4,9}/(?!/)[-._;()/:A-Za-z0-9]+(?=\s|$)";
}

public static class TextAnalyzer
{
    public static List<SearchResult> FindMatches(string text, string pattern)
    {
        var results = new List<SearchResult>();

        if (string.IsNullOrWhiteSpace(text)) 
            return results;
        
        var regex = new Regex(
            pattern,
            RegexOptions.Multiline |
            RegexOptions.CultureInvariant
        );

        foreach (Match m in regex.Matches(text))
        {
            int line = GetLineNumber(text, m.Index);
            int col = GetColumnNumber(text, m.Index);

            results.Add(new SearchResult
            {
                Fragment = m.Value,
                Line = line,
                Column = col,
                Length = m.Length,
                StartIndex = m.Index
            });
        }

        return results;
    }

    private static int GetLineNumber(string text, int index)
        => text.Take(index).Count(c => c == '\n') + 1;

    private static int GetColumnNumber(string text, int index)
    {
        int lastNewline = text.LastIndexOf('\n', index);
        return lastNewline == -1 ? index + 1 : index - lastNewline;
    }
}

public class SearchResult
{
    public string Fragment { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }
    public int StartIndex { get; set; }
}
  