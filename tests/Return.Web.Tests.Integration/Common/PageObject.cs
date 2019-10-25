// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageObject.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using OpenQA.Selenium;

    public abstract class PageObject : IPageObject {
        private bool _ownsWebdriver;
        private WebDriverContainer _webDriverContainer;

        public IWebDriver WebDriver => this._webDriverContainer?.WebDriver;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1033:Interface methods should be callable by child types",
            Justification = "Not necessary for testing framework")]
        void IPageObject.SetWebDriver(WebDriverContainer webDriver) {
            this._webDriverContainer = webDriver;
            this._ownsWebdriver = true;
        }

        public void Unfocus() {
            TestContext.WriteLine("Unfocus by sending tab");
            this.WebDriver.FindElement(By.CssSelector("body")).SendKeys("\t");
        }

        public void InitializeFrom(PageObject owner) {
            this._webDriverContainer = owner._webDriverContainer;
            this._ownsWebdriver = false;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (this._ownsWebdriver) {
                    this._webDriverContainer?.Dispose();
                }

                this._webDriverContainer = null;
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IPageObject : IDisposable {
        void SetWebDriver(WebDriverContainer webDriver);

        IWebDriver WebDriver { get; }
    }
}
