// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : WebDriverExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public static class WebDriverExtensions {
        public static IWebElement FindElementByTestElementId(this IWebDriver webDriver, string testElementId) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));

            By selector = By.CssSelector($"[data-test-element-id=\"{testElementId}\"]");
            return webDriver.FindVisibleElement(selector);
        }

        public static IWebElement FindVisibleElement(this IWebDriver webDriver, By selector) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));

            return new DefaultWait<IWebDriver>(webDriver).Until(wd => {
                var el = wd.FindElement(selector);
                if (el != null && el.Displayed) {
                    return el;
                }

                return null;
            });
        }

        public static void NavigateToBlazorPage(this IWebDriver webDriver, Uri uri) {
            webDriver.Navigate().GoToUrl(uri);

            // Allow some time for the page to initialize
            Thread.Sleep(1000);
        }
    }
}
