// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [RetryTest(3)]
    public abstract class PageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {

        protected TPageObject Page { get; private set; }

        public override void OnInitialized() => this.Page = this.App.CreatePageObject<TPageObject>();
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Page?.Dispose();
            }
        }

        [TearDown]
        public void CreateScreenshots() {
            this.Page?.WebDriver?.TryCreateScreenshot("Client_AfterTest");
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    [RetryTest(3)]
    public abstract class TwoClientPageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {
        protected TPageObject Client1 { get; private set; }
        protected TPageObject Client2 { get; private set; }

        public override void OnInitialized() {
            this.Client1 = this.App.CreatePageObject<TPageObject>();
            this.Client2 = this.App.CreatePageObject<TPageObject>();
        }

        [TearDown]
        public void CreateScreenshots() {
            this.Client1?.WebDriver?.TryCreateScreenshot("Client1_AfterTest");
            this.Client2?.WebDriver?.TryCreateScreenshot("Client1_AfterTest");
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Client1?.Dispose();
                this.Client2?.Dispose();
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void MultiAssert(Action<TPageObject> action) =>
            Task.WaitAll(
                Task.Run(() => action.Invoke(this.Client1)),
                Task.Run(() => action.Invoke(this.Client2))
            );
    }

    [UseRunningApp]
    [CleanupDisposables]
    public abstract class ScopedFixture : IAppFixture {
        public ReturnAppFactory App { get; set; }
        public virtual void OnInitialized() { }

        protected IServiceScope ServiceScope { get; private set; }

        [SetUp] public void SetUpServiceScope() => this.ServiceScope = this.App.Services.CreateScope();

        [TearDown]
        public void KillServiceScope() {
            this.ServiceScope?.Dispose();
            this.ServiceScope = null;
        }
    }
}
