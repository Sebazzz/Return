﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : WebDriverExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Threading;
    using NUnit.Framework;
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

            return webDriver.Retry(wd => {
                IWebElement el = wd.FindElement(selector);
                return el != null && el.Displayed ? el : null;
            });
        }

        public static void NavigateToBlazorPage(this IWebDriver webDriver, Uri uri) {
            webDriver.Navigate().GoToUrl(uri);

            // Allow some time for the page to initialize
            Thread.Sleep(1000);
        }

        public static T Retry<T>(this IWebDriver webDriver, Func<IWebDriver, T> callback) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));

            try {
                var wait = new DefaultWait<IWebDriver>(webDriver);
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException),
                    typeof(NoSuchFrameException),
                    typeof(NoSuchWindowException),
                    typeof(StaleElementReferenceException));
                return wait.Until(callback);
            }
            catch (Exception ex) {
                TestContext.WriteLine($"Exception while waiting on Retry<{typeof(T)}> operation: {ex}");
                webDriver.TryLogContext();

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log and continue, we will not fail on logging")]
        public static void TryLogContext(this IWebDriver webDriver) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));
            try {
                TestContext.WriteLine($"Current URL: {webDriver.Url}");
                TestContext.WriteLine($"Current HTML: {webDriver.PageSource}");
            }
            catch (Exception ex) {
                TestContext.WriteLine($"Unable to log contextual info: {ex}");
            }

            try {
                ILogs logs = webDriver.Manage().Logs;

                foreach (string logType in logs.AvailableLogTypes) {
                    TestContext.WriteLine($"Browser log: {logType}");

                    foreach (LogEntry logEntry in logs.GetLog(logType)) {
                        TestContext.WriteLine($"\t[{logEntry.Level}] [{logEntry.Timestamp:s}] {logEntry.Message}");
                    }
                }
            }
            catch (Exception ex) {
                TestContext.WriteLine($"Unable to log browser log info: {ex}");
            }
        }
    }
}
