// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyTestsBase.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************
namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Services;
    using Common;
    using Domain.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium.Support.UI;

    public class RetrospectiveLobbyTestsBase : TwoClientPageFixture<RetrospectiveLobby> {
        protected string RetroId { get; set; }
        private int _colorIndex = 1;

        [SetUp]
        public void ResetColorIndex() => this._colorIndex = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object does not have ownership")]
        protected void Join(RetrospectiveLobby pageObject, bool facilitator) {
            var joinPage = new JoinRetrospectivePage();
            ((IPageObject)joinPage).SetWebDriver(pageObject.WebDriver);
            joinPage.Navigate(this.App, this.RetroId);

            joinPage.NameInput.SendKeys(Name.Create());

            Thread.Sleep(500);
            new SelectElement(joinPage.ColorSelect).SelectByIndex(this._colorIndex++);

            if (facilitator) {
                joinPage.IsFacilitatorCheckbox.Click();
                Thread.Sleep(500);
                joinPage.WebDriver.Retry(_ => {
                    joinPage.FacilitatorPassphraseInput.SendKeys("scrummaster");
                    return true;
                });
            }

            Thread.Sleep(500);
            joinPage.Submit();
            Thread.Sleep(500);
        }

        protected void WaitNavigatedToLobby() =>
            Task.WaitAll(
                Task.Run(() => WaitNavigatedToLobby(this.Client1)),
                Task.Run(() => WaitNavigatedToLobby(this.Client2))
            );

        protected async Task SetRetrospective(Action<Retrospective> action) {
            using IServiceScope scope = this.App.CreateTestServiceScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IReturnDbContext>();

            var retrospective = await dbContext.Retrospectives.FindByRetroId(this.RetroId, CancellationToken.None);
            action.Invoke(retrospective);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private static void WaitNavigatedToLobby(RetrospectiveLobby pageObject) {
            var sw = new Stopwatch();
            sw.Start();

            Assume.That(() => pageObject.WebDriver.Url, Does.Match("/lobby").Retry(), "We didn't navigate to the lobby");
            Assume.That(() => pageObject.WebDriver.Retry(wd => wd.FindElementByTestElementId("main-board").Displayed), Is.True.Retry(count: 2), "The retrospective board does not load");

            sw.Stop();
            TestContext.WriteLine($"Navigated to lobby in {sw.Elapsed}");
        }
    }
}
