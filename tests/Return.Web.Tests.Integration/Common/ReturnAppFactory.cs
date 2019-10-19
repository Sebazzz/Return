// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnAppFactory.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Linq;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.Events;

    public class ReturnAppFactory : CustomWebApplicationFactory<Startup> {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "API consistency / design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IWebDriver is disposed by child")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log and continue, we will not fail on logging")]
        public IWebDriver CreateWebDriver() {
            var webDriverOptions = new ChromeOptions {
                PageLoadStrategy = PageLoadStrategy.Normal,
                AcceptInsecureCertificates = true,
            };


            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("MOZ_HEADLESS"))) {
                TestContext.WriteLine("Going to run Chrome headless");
                webDriverOptions.AddArguments("--headless");
            }

            var webDriver = new ChromeDriver(webDriverOptions);

            ITimeouts timeouts = webDriver.Manage().Timeouts();
            timeouts.ImplicitWait = TimeSpan.FromSeconds(10);
            timeouts.PageLoad = TimeSpan.FromSeconds(10);



            void WrapLoggerAction<TArgs>(TArgs args, Action act) {
                try {
                    act();
                }
                catch (Exception ex) {
                    TestContext.WriteLine($"Cannot log action {args.ToString()}: [{ex.GetType().FullName}] {ex.Message}");
                }
            }

            var wrapper = new EventFiringWebDriver(webDriver);
            wrapper.ElementClicked += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: click - {args.Element.TagName}"));
            wrapper.ElementClicking += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: clicking - {args.Element.TagName}"));

            wrapper.ExceptionThrown += (_, args) => {
                TestContext.WriteLine($"WebDriver: exception - {args.ThrownException}");
                webDriver.TryLogContext();
            };

            wrapper.FindingElement += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: finding element - {args.FindMethod}"));
            wrapper.FindElementCompleted += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: finding element completed - {args.FindMethod}"));
            wrapper.Navigating += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: navigating - {args.Url}"));
            wrapper.Navigated += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: navigated - {args.Url}"));
            wrapper.ElementValueChanging += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: element value changing - {args.Element.TagName} [{args.Element.GetProperty("outerHTML")}]"));
            wrapper.ElementValueChanged += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: element value changed - {args.Element.TagName} [{args.Element.GetProperty("outerHTML")}]"));

            return wrapper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IPageObject is disposable itself")]
        public TPageObject CreatePageObject<TPageObject>() where TPageObject : IPageObject, new() {
            var pageObject = Activator.CreateInstance<TPageObject>();
            pageObject.SetWebDriver(this.CreateWebDriver());
            return pageObject;
        }

        public Uri CreateUri(string path) => new Uri(this.Server.BaseAddress, path);
    }
}
