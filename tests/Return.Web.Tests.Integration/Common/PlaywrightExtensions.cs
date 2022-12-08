// ******************************************************************************
//  © 2022 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PlaywrightExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

internal static class PlaywrightExtensions
{
    public static Task GotoAsync(this IPage page, Uri url, PageGotoOptions options = default) => page.GotoAsync(url.ToString(), options);

    public static Task GotoBlazorPageAsync(this IPage page, Uri url, PageGotoOptions options = default) =>
        page.RunAndWaitForWebSocketAsync(() => page.GotoAsync(url, options));

    public static ILocator FindElementByTestElementId(this IPage browserPage, string testElementId) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        return browserPage.Locator($"[data-test-element-id=\"{testElementId}\"]");
    }

    private static int ScreenshotCounter = 0;
    public static async Task TryCreateScreenshot(this IPage browserPage, string extraName = null)
    {
        string screenshotName = TestContext.CurrentContext.Test.MethodName + "-" + (++ScreenshotCounter) + (extraName != null ? "-" + extraName : "") + ".png";
        string screenshotPath = Path.Join(Paths.TestArtifactDir, screenshotName);

        try {
            TestContext.WriteLine($"Creating screenshot: {screenshotPath}");
            await browserPage.ScreenshotAsync(new()
            {
                FullPage = true,
                Type = ScreenshotType.Png,
                Path = screenshotPath
            });
        }
        catch (Exception ex) {
            TestContext.WriteLine($"--> Unable to create screenshot: {ex}");
        }
    }


    public static ILocatorAssertions Expected(this ILocator locator) => Assertions.Expect(locator);

    public static IPageAssertions Expected(this IPage page) => Assertions.Expect(page);

    public static IAPIResponseAssertions Expected(this IAPIResponse response) => Assertions.Expect(response);
}
