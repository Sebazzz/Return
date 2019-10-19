// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PageFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    public abstract class PageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {

        protected TPageObject Page { get; private set; }

        public override void OnInitialized() => this.Page = this.App.CreatePageObject<TPageObject>();
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

    [UseRunningApp]
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
