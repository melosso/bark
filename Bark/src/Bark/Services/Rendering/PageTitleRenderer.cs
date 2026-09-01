using System.Text.RegularExpressions;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class PageTitleRenderer
{
    private static readonly Regex TokenPattern = new(@":title|:siteName", RegexOptions.Compiled);

    public static string ComputeTitle(string pageTitle, Config? config, Localization? localization = null)
    {
        var l = localization ?? Localization.Default;
        var template = l.Label(config?.TitleTemplate);
        var siteName = l.Label(config?.Title);


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
