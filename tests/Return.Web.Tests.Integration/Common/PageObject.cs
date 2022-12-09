// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageObject.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

public abstract class PageObject : IPageObject {
    private bool _ownsWebdriver;
    private IBrowserContext _browserContext;

    public IBrowserContext Browser => this._browserContext;
    public IPage BrowserPage => this._browserContext.Pages.Single();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
        "CA1033:Interface methods should be callable by child types",
        Justification = "Not necessary for testing framework")]
    void IPageObject.SetBrowserContext(IBrowserContext browserContext) {
        this._browserContext = browserContext;
        this._ownsWebdriver = true;
    }

    public Task Unfocus() {
        TestContext.WriteLine("Unfocus by sending tab");
        return this.BrowserPage.Keyboard.PressAsync("Tab");
    }

    public void InitializeFrom(PageObject owner) {
        this._browserContext = owner._browserContext;
        this._ownsWebdriver = false;
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (this._ownsWebdriver) {
                _ = this._browserContext?.CloseAsync();
            }
        }
    }

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public interface IPageObject : IDisposable {
    void SetBrowserContext(IBrowserContext browserContext);
    IPage BrowserPage { get; }
}
