// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : WebDriverPool.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Collections.Concurrent;
    using NUnit.Framework;
    using OpenQA.Selenium;

    internal sealed class WebDriverPool : IDisposable {
        private readonly Func<IWebDriver> _factory;
        private readonly ConcurrentBag<IWebDriver> _webDrivers;

        public WebDriverPool(Func<IWebDriver> factory) {
            this._factory = factory;
            this._webDrivers = new ConcurrentBag<IWebDriver>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Lifetime will be managed externally")]
        public IWebDriver Get() => this._webDrivers.TryTake(out IWebDriver webDriver) ? webDriver : this._factory.Invoke();

        public void Return(IWebDriver webDriver) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));
            this._webDrivers.Add(webDriver);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Dispose the web drivers should all be done")]
        public void Dispose() {
            while (this._webDrivers.TryTake(out IWebDriver driver)) {
                try {
                    driver.Close();
                }
                catch (Exception ex) {
                    TestContext.WriteLine($"Unable to dispose web driver: {ex}");
                }
                finally {
                    driver.Dispose();
                }
            }
        }
    }
}
