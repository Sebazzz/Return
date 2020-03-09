// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnAppFactory.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Drawing;
    using System.Linq;
    using Domain.Abstractions;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.Events;
    using Persistence;

    public sealed class ReturnAppFactory : CustomWebApplicationFactory<Startup> {
        private WebDriverPool _webDriverPool;

        public ReturnAppFactory() {
            this._webDriverPool = new WebDriverPool(this.CreateWebDriver);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            this._webDriverPool?.Dispose();
            this._webDriverPool = null;
        }

        public WebDriverContainer GetWebDriver() => new WebDriverContainer(this._webDriverPool.Get(), this);

        internal void Return(IWebDriver webDriver) => this._webDriverPool.Return(webDriver);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "API consistency / design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IWebDriver is disposed by child")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log and continue, we will not fail on logging")]
        private IWebDriver CreateWebDriver() {
            var webDriverOptions = new ChromeOptions {
                PageLoadStrategy = PageLoadStrategy.Normal,
                AcceptInsecureCertificates = true,
            };

            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("MOZ_HEADLESS"))) {
                TestContext.WriteLine("Going to run Chrome headless");
                webDriverOptions.AddArguments("--headless");
            }

            var webDriver = new ChromeDriver(webDriverOptions);

            var window = webDriver.Manage().Window;
            try {
                window.Size = new Size(1920, 1080);
            }
            catch (Exception ex) {
                TestContext.WriteLine($"Setting driver window size not supported: {ex}");
            }

            // Overridable timeout for tests for known CI failures
            if (!Int32.TryParse(Environment.GetEnvironmentVariable("RETURN_TEST_WAIT_TIME"), out int waitTime)) {
                waitTime = 10;
            }

            TestContext.WriteLine($"Configuration of WebDriver using wait time: {waitTime}s");
            ITimeouts timeouts = webDriver.Manage().Timeouts();
            timeouts.ImplicitWait = TimeSpan.FromSeconds(waitTime);
            timeouts.PageLoad = TimeSpan.FromSeconds(waitTime);

            void WrapLoggerAction<TArgs>(TArgs args, Action act, string screenshotName = null) {
                try {
                    if (screenshotName != null) webDriver.TryCreateScreenshot($"action-{screenshotName}");

                    act();
                }
                catch (Exception ex) {
                    TestContext.WriteLine($"Cannot log action {args.ToString()}: [{ex.GetType().FullName}] {ex.Message}");
                }
            }

            var wrapper = new EventFiringWebDriver(webDriver);
            wrapper.ElementClicked += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: click - {args.Element.TagName}"), "element-clicked");
            wrapper.ElementClicking += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: clicking - {args.Element.TagName}"), "element-clicking");

            wrapper.ExceptionThrown += (_, args) => {
                TestContext.WriteLine($"WebDriver: exception - {args.ThrownException}");
                webDriver.TryCreateScreenshot("exception-" + args.ThrownException.GetType().Name);
                webDriver.TryLogContext();
            };

            wrapper.FindingElement += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: finding element - {args.FindMethod}"));
            wrapper.FindElementCompleted += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: finding element completed - {args.FindMethod}"));
            wrapper.Navigating += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: navigating - {args.Url}"), "navigate");
            wrapper.Navigated += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: navigated - {args.Url}"), "navigate");
            wrapper.ElementValueChanging += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: element value changing - {args.Element.TagName} [{args.Element.GetProperty("outerHTML")}]"));
            wrapper.ElementValueChanged += (_, args) => WrapLoggerAction(args, () => TestContext.WriteLine($"WebDriver: element value changed - {args.Element.TagName} [{args.Element.GetProperty("outerHTML")}]"));

            return wrapper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IPageObject is disposable itself")]
        public TPageObject CreatePageObject<TPageObject>() where TPageObject : IPageObject, new() {
            var pageObject = Activator.CreateInstance<TPageObject>();
            pageObject.SetWebDriver(this.GetWebDriver());
            return pageObject;
        }

        public int GetId<TEntity>(Func<DbSet<TEntity>, int> query) where TEntity : class {
            using IServiceScope scope = this.CreateTestServiceScope();

            var returnDbContext = scope.ServiceProvider.GetRequiredService<ReturnDbContext>();
            DbSet<TEntity> dbSet = returnDbContext.Set<TEntity>();

            return query.Invoke(dbSet);
        }

        public int GetLastAddedId<TEntity>() where TEntity : class, IIdPrimaryKey => this.GetId<TEntity>(dbSet => dbSet.OrderByDescending(x => x.Id).Select(x => x.Id).First());

        public Uri CreateUri(string path) => new Uri(this.Server.BaseAddress, path);

        public IServiceScope CreateTestServiceScope() => this.Services.CreateScope();

        protected override string ConnectionString { get; } = (new SqliteConnectionStringBuilder {
            BrowsableConnectionString = true,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.Memory,
            ForeignKeys = true,
            DataSource = "testdb1"
        }).ToString();
    }
}
