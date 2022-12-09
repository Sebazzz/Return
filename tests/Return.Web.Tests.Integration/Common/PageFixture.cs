// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

[RetryTest(3)]
public abstract class PageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {
    protected TPageObject Page { get; private set; }

    public override async Task OnInitialized() => this.Page = await this.App.CreatePageObject<TPageObject>();
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            this.Page?.Dispose();
        }
    }

    [TearDown]
    public Task CreateScreenshots() => this.Page?.BrowserPage?.TryCreateScreenshot("Client_AfterTest");

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}

[RetryTest(3)]
public abstract class TwoClientPageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {
    protected TPageObject Client1 { get; private set; }
    protected TPageObject Client2 { get; private set; }

    public override async Task OnInitialized() {
        this.Client1 = await this.App.CreatePageObject<TPageObject>();
        this.Client2 = await this.App.CreatePageObject<TPageObject>();
    }

    [SetUp]
    public async Task StartTracing() {
        await this.Client1.BrowserPage.StartTrace("_Client1");
        await this.Client2.BrowserPage.StartTrace("_Client2");
    }

    [TearDown]
    public async Task StopTracing() {
        Task t1 = this.Client1?.BrowserPage?.TryCreateScreenshot("Client1_AfterTest");
        if (t1 is not null) await t1;

        Task t2 =  this.Client2?.BrowserPage?.TryCreateScreenshot("Client1_AfterTest");
        if (t2 is not null) await t2;

        if (this.Client1 != null) await this.Client1.BrowserPage.StopTrace("_Client1");
        if (this.Client2 != null) await this.Client2.BrowserPage.StopTrace("_Client2");
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            this.Client1?.Dispose();
            this.Client2?.Dispose();
        }
    }

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected Task MultiAssert(Func<TPageObject, Task> action) =>
        Task.WhenAll(
            action.Invoke(this.Client1),
            action.Invoke(this.Client1));
}

[UseRunningApp]
[CleanupDisposables]
public abstract class ScopedFixture : PlaywrightTest, IAppFixture {
    public ReturnAppFactory App { get; set; }
    public virtual Task OnInitialized() => Task.CompletedTask;

    protected IServiceScope ServiceScope { get; private set; }

    [SetUp] public void SetUpServiceScope() => this.ServiceScope = this.App.Services.CreateScope();

    [TearDown]
    public void KillServiceScope() {
        this.ServiceScope?.Dispose();
        this.ServiceScope = null;
    }
}
