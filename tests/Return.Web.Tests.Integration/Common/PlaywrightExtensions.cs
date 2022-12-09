// ******************************************************************************
//  © 2022 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PlaywrightExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using Return.Common;

internal static class PlaywrightExtensions
{
    public static Task GotoAsync(this IPage page, Uri url, PageGotoOptions options = default) => page.GotoAsync(url.ToString(), options);

    public static Task GotoBlazorPageAsync(this IPage page, Uri url, PageGotoOptions options = default) =>
        page.RunAndWaitForWebSocketAsync(() => page.GotoAsync(url, options));

    public static ILocator FindElementByTestElementId(this IPage browserPage, string testElementId) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        return browserPage.Locator($"[data-test-element-id=\"{testElementId}\"]");
    }

    public static ILocator FindElementByTestElementId(this ILocator browserPage, string testElementId) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        return browserPage.Locator($"[data-test-element-id=\"{testElementId}\"]");
    }

    public static ILocator FindElementByTestElementId(this IPage browserPage, string testElementId, int id) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        return browserPage.Locator($"[data-test-element-id=\"{testElementId}\"][data-id=\"{id}\"]");
    }

    public static ILocator FindElementByTestElementId(this ILocator browserPage, string testElementId, int id) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        return browserPage.Locator($"[data-test-element-id=\"{testElementId}\"][data-id=\"{id}\"]");
    }

    public static async Task<T> GetAttributeAsync<T>(this ILocator webElement, string attributeName) {
        try {
            return (T)Convert.ChangeType(await webElement.GetAttributeAsync(attributeName), typeof(T), Culture.Invariant);
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"Cannot read attribute '{attributeName}' of element {(await webElement.ElementHandleAsync())} as {typeof(T)}", ex);
        }
    }

    public static async Task<List<T>> GenerateSubElementsByTestElementId<T>(this ILocator browserPage, string testElementId, Func<ILocator, Task<T>> factory) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        ILocator locator = browserPage.Locator($"[data-test-element-id=\"{testElementId}\"]");
        await locator.First.Expected().ToBeVisibleAsync();

        int cnt = await locator.CountAsync();
        List<T> items = new List<T>(cnt);
        for (int i = 0; i < cnt; i++)
        {
            items.Add(await factory.Invoke(locator.Nth(i)));
        }

        return items;
    }

    public static async Task<List<T>> GenerateSubElementsByTestElementId<T>(this ILocator browserPage, string testElementId, Func<ILocator, T> factory) {
        if (browserPage == null) throw new ArgumentNullException(nameof(browserPage));
        ILocator locator = browserPage.Locator($"[data-test-element-id=\"{testElementId}\"]");
        await locator.First.Expected().ToBeVisibleAsync();

        int cnt = await locator.CountAsync();
        List<T> items = new List<T>(cnt);
        for (int i = 0; i < cnt; i++)
        {
            items.Add(factory.Invoke(locator.Nth(i)));
        }

        return items;
    }

    public static Task StartTrace(this IPage page, string suffix = default) {
        if (!Debugger.IsAttached) return Task.CompletedTask;

        return page.Context.Tracing.StartAsync(new()
        {
            Name = "_" + TestContext.CurrentContext.Test.Name + suffix,
            Screenshots = true,
            Snapshots = true,
            Sources = true,
            Title = TestContext.CurrentContext.Test.FullName
        });
    }

    public static Task StopTrace(this IPage page, string suffix = default)
    {
        if (!Debugger.IsAttached) return Task.CompletedTask;

        string fileName = Path.Combine(Paths.TracesDirectory, TestContext.CurrentContext.Test.Name + suffix + ".zip");
        TestContext.WriteLine("Saving trace. Use command to view:");
        TestContext.WriteLine($"\tplaywright show-trace \"{fileName}\"");
        return page.Context.Tracing.StopAsync(new() { Path = fileName });
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
