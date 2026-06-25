using Bark.Models;
using Bark.Services;

namespace Bark.Tests;

public sealed class BarkContainerRendererTests
{
    private const string CodeGroupMd = "::: code-group\n```sh [npm]\nnpm install\n```\n```sh [pnpm]\npnpm install\n```\n:::\n";

    [Fact]
    public void CodeGroup_IconsEnabledByDefault_RendersImgWithSlugifiedTitle()
    {
        var options = new CodeGroupIconOptions();
        var service = new MarkdownService(codeGroupIcons: options);
        var (html, _, _, _) = service.Parse(CodeGroupMd);

        Assert.Contains($"{options.BaseUrl}/npm.{options.Format}", html);
        Assert.Contains($"{options.BaseUrl}/pnpm.{options.Format}", html);
    }

    [Fact]
    public void CodeGroup_IconsDisabled_NoImgTag()
    {
        var options = new CodeGroupIconOptions { Enabled = false };
        var service = new MarkdownService(codeGroupIcons: options);
        var (html, _, _, _) = service.Parse(CodeGroupMd);
        Assert.DoesNotContain("<img", html);
    }

    [Fact]
    public void CodeGroup_OverrideWinsOverSlugifiedTitle()
    {
        var options = new CodeGroupIconOptions
        {
            Overrides = new Dictionary<string, string> { ["npm"] = "nodedotjs" }
        };
        var service = new MarkdownService(codeGroupIcons: options);
        var (html, _, _, _) = service.Parse(CodeGroupMd);

        Assert.Contains($"{options.BaseUrl}/nodedotjs.{options.Format}", html);
    }
}
