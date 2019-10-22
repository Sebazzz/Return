// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageObject.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using OpenQA.Selenium;

    public abstract class PageObject : IPageObject {
        public IWebDriver WebDriver { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "Not necessary for testing framework")]
        void IPageObject.SetWebDriver(IWebDriver webDriver) => this.WebDriver = webDriver;

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.WebDriver?.Close();
                this.WebDriver?.Dispose();
                this.WebDriver = null;
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IPageObject : IDisposable {
        void SetWebDriver(IWebDriver webDriver);

        IWebDriver WebDriver { get; }
    }
}
