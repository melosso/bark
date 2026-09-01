namespace Bark.Configuration;

public sealed record CliArguments(
    string? ExportDir,
    string? ExportBaseUrl,
    string? BasePath,
    string? Theme,
    string? TranslateTo = null,
    string? TranslateEndpoint = null,
    string? TranslateFrom = null,
    string? TranslateApiKey = null,
    bool TranslateOverwrite = false)
{
    public static CliArguments Parse(string[] args)
    {
        string? exportDir = null;
        string? exportBaseUrl = null;
        string? basePath = null;
        string? theme = null;
        string? translateTo = null;
        string? translateEndpoint = null;
        string? translateFrom = null;
        string? translateApiKey = null;
        var translateOverwrite = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--export" when i + 1 < args.Length: exportDir = args[++i]; break;
                case "--base-url" when i + 1 < args.Length: exportBaseUrl = args[++i]; break;
                case "--base-path" when i + 1 < args.Length: basePath = args[++i]; break;
                case "--theme" when i + 1 < args.Length: theme = args[++i]; break;
                case "--translate" when i + 1 < args.Length: translateTo = args[++i]; break;
                case "--translate-endpoint" when i + 1 < args.Length: translateEndpoint = args[++i]; break;
                case "--translate-from" when i + 1 < args.Length: translateFrom = args[++i]; break;
                case "--translate-api-key" when i + 1 < args.Length: translateApiKey = args[++i]; break;
                case "--translate-overwrite": translateOverwrite = true; break;
            }
        }

        return new CliArguments(exportDir, exportBaseUrl, basePath, theme, translateTo, translateEndpoint, translateFrom, translateApiKey, translateOverwrite);
    }
}
