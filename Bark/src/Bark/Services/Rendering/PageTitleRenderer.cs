using System.Text.RegularExpressions;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class PageTitleRenderer
{
    private static readonly Regex TokenPattern = new(@":title|:siteName", RegexOptions.Compiled);

    public static string ComputeTitle(string pageTitle, Config? config)
    {
        var template = config?.TitleTemplate;
        var siteName = config?.Title;

        if (template is not null)
        {
            return TokenPattern.Replace(template, m => m.Value switch
            {
                ":title" => pageTitle,
                ":siteName" => siteName ?? string.Empty,
                _ => m.Value
            });
        }

        return siteName is not null ? $"{pageTitle} | {siteName}" : pageTitle;
    }
}
