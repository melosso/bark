namespace Bark.Models;

public sealed record PageControlsConfig
{
    public bool DownloadMarkdown { get; init; }
    public OpenInEditorConfig? OpenInEditor { get; init; }
}

public sealed record OpenInEditorConfig
{
    /// <summary>URL template; <c>{path}</c> is replaced with the page's relative .md file path.</summary>
    public required string Template { get; init; }
    public string Label { get; init; } = "Open in editor";
}
