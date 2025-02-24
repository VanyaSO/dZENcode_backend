using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace dZENcode_backend.Helpers;

public static class HtmlValidator
{
    private static readonly string[] _allowedTags = ["a", "code", "i", "strong"];

    public static (bool, string?) IsValid(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        Regex openTagsRegex = new Regex("<([a-z]+)[^>]*?>", RegexOptions.IgnoreCase);
        Regex closeTagsRegex = new Regex("</([a-z]+)>", RegexOptions.IgnoreCase);
        if (openTagsRegex.IsMatch(html) == closeTagsRegex.IsMatch(html))
        {
            Regex contentRegex = new Regex(@"(?!\s*<(i|code|strong|a)(?:\s[^>]*)?>\s*<\/\1>\s*$).+", RegexOptions.IgnoreCase);
            if (contentRegex.IsMatch(html))
            {
                foreach (var node in doc.DocumentNode.Descendants())
                {
                    if (node.NodeType == HtmlNodeType.Element && !_allowedTags.Contains(node.Name))
                        return (false, $"Тег <{node.Name}> не разрешен");

                    if (node.Name == "a" && node.Attributes.Count <= 2 && (!node.Attributes.Contains("href") || !node.Attributes.Contains("title")))
                        return (false, "Тег <a> должен содержать атрибуты href и title");
                }
                
                return (true, null);
            }

            return (false, "Теги не могут быть пустыми.");
        }
        return (false, "Все теги должны быть закрыты");
    }
}