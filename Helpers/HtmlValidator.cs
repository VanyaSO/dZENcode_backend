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

        Regex openTags = new Regex("<([a-z]+)[^>]*?>", RegexOptions.IgnoreCase);
        Regex closeTags = new Regex("</([a-z]+)>", RegexOptions.IgnoreCase);
        if (openTags.IsMatch(html) == closeTags.IsMatch(html))
        {
            foreach (var node in doc.DocumentNode.Descendants())
            {
                if (node.NodeType == HtmlNodeType.Element && !_allowedTags.Contains(node.Name))
                    return (false, $"Тег <{node.Name}> не разрешен");

                if (node.Name == "a" && node.Attributes.Count != 2 && !node.Attributes.Contains("href") && !node.Attributes.Contains("title"))
                    return (false, "Тег <a> должен содержать атрибуты href и title");
            }
        
            return (true, null);
        }
        return (false, "Все теги должны быть закрыты");
    }
}