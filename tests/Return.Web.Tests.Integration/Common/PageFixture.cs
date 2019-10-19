// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using OpenQA.Selenium;

    [UseRunningApp]
    public abstract class PageFixture<TPageObject> : IDisposable, IAppFixture where TPageObject : IPageObject, new() {
        public ReturnAppFactory App { get; set; }
        public IWebDriver WebDriver { get; set; }

        protected TPageObject Page { get; private set; }

        public void OnInitialized() => this.Page = this.App.CreatePageObject<TPageObject>();

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Page?.Dispose();
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
