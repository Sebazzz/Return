// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : WebDriverContainer.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using OpenQA.Selenium;

    public sealed class WebDriverContainer : IDisposable {
        private readonly ReturnAppFactory _owner;
        private IWebDriver _webDriver;

        internal WebDriverContainer(IWebDriver webDriver, ReturnAppFactory owner) {
            this._webDriver = webDriver;
            this._owner = owner;
        }

        public IWebDriver WebDriver => this._webDriver ?? throw new ObjectDisposedException(this.ToString());

        public void Dispose() {
            if (this._webDriver != null) {
                this._owner.Return(this._webDriver);
                this._webDriver = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
