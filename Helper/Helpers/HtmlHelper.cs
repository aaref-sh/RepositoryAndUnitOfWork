using System.Text.RegularExpressions;

namespace Helper.Helpers;

public record HtmlElementReplacement(string ElementId, string Replacement);

public class HtmlModifier
{
    public static string ReplaceTagsContent(string html, List<HtmlElementReplacement> replacements)
    {
        string elements = replacements.Select(x => Regex.Escape(x.ElementId)).ToArray().JoinStr("|");
        string pattern = $@"(<[^>]+id=['""]({elements})['""][^>]*>)(.*?)(<\/[^>]+>)";

        var res = Regex.Replace(html, pattern, match =>
        {
            var elementId = match.Groups[2].Value;
            var replacement = replacements.FirstOrDefault(x => x.ElementId == elementId)?.Replacement;
            return $"{match.Groups[1].Value}{replacement}{match.Groups[4].Value}";
        });
        return res;
    }
}
