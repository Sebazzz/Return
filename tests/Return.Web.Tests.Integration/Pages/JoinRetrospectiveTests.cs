// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Application.Retrospectives.Queries.GetParticipantsInfo;
    using Common;
    using Components;
    using Domain.Entities;
    using Domain.Services;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public sealed class JoinRetrospectiveTests : PageFixture<JoinRetrospectivePage> {
        [Test]
        public void JoinRetrospectivePage_UnknownRetrospective_ShowNotFoundMessage() {
            // Given
            string retroIdentifier = new RetroIdentifierService().CreateNew().StringId;

            // When
            this.Page.Navigate(this.App, retroIdentifier);

            // Then
            Assert.That(this.Page.Title.Text, Contains.Substring("not found"));
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospective_FormShownWithValidation() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            this.Page.Navigate(this.App, retroId);

            // When
            this.Page.Submit();

            // Then
            string[] messages = new DefaultWait<JoinRetrospectivePage>(this.Page)
                .Until(p => {
                    ReadOnlyCollection<IWebElement> collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contain("'Name' must not be empty"));
            Assert.That(messages, Has.One.Contain("This passphrase is not valid. Please try again"));
            Assert.That(messages, Has.One.Contain("Please select a color"));
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospectiveAlreadyStarted_ShowMessage() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            await this.SetRetrospective(retroId, retro => retro.CurrentStage = RetrospectiveStage.Writing);

            // When
            this.Page.Navigate(this.App, retroId);

            // Then
            Assert.That(() => this.Page.WebDriver.FindElements(By.CssSelector(".notification.is-info")), Has.Count.EqualTo(1).Retry());
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospectiveFinished_ShowMessage() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            await this.SetRetrospective(retroId, retro => retro.CurrentStage = RetrospectiveStage.Finished);

            // When
            this.Page.Navigate(this.App, retroId);

            // Then
            Assert.That(() => this.Page.WebDriver.FindElements(By.CssSelector(".notification.is-warning")), Has.Count.EqualTo(1).Retry());
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospective_ValidatesParticipantPassphaseAndRedirectsToLobby() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            string myName = Name.Create();
            this.Page.Navigate(this.App, retroId);

            // When
            this.Page.NameInput.SendKeys(myName);
            new SelectElement(this.Page.ColorSelect).SelectByIndex(1);
            this.Page.ParticipantPassphraseInput.SendKeys("secret");
            this.Page.Submit();

            // Then
            Assert.That(() => this.Page.WebDriver.Url, Does.Match("/retrospective/" + retroId + "/lobby").Retry());
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospective_JoinParticipantUpdatesParticipantListInRealtime() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            string myName = Name.Create();
            this.Page.Navigate(this.App, retroId);

            var secondInstance = this.App.CreatePageObject<JoinRetrospectivePage>().RegisterAsTestDisposable();
            secondInstance.Navigate(this.App, retroId);

            // When
            this.Page.NameInput.SendKeys(myName);
            new SelectElement(this.Page.ColorSelect).SelectByIndex(1);
            this.Page.ParticipantPassphraseInput.SendKeys("secret");
            this.Page.Submit();

            // Then
            Assert.That(() => secondInstance.OnlineList.OnlineListItems.Select(x => x.Text), Has.One.Contains(myName));
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospective_JoinParticipantUpdatesColorListInRealtime() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            string myName = Name.Create();
            this.Page.Navigate(this.App, retroId);

            JoinRetrospectivePage secondInstance = this.App.CreatePageObject<JoinRetrospectivePage>().RegisterAsTestDisposable();
            secondInstance.Navigate(this.App, retroId);

            IList<AvailableParticipantColorModel> availableColors;
            {
                using IServiceScope scope = this.App.CreateTestServiceScope();
                scope.SetNoAuthenticationInfo();
                availableColors = await scope.Send(new GetAvailablePredefinedParticipantColorsQuery(retroId));
            }
            AvailableParticipantColorModel colorToSelect = availableColors[TestContext.CurrentContext.Random.Next(0, availableColors.Count)];

            // When
            var selectList = new SelectElement(this.Page.ColorSelect);
            Assert.That(() => selectList.Options.Select(x => x.GetProperty("value")).Where(x => !String.IsNullOrEmpty(x)), Is.EquivalentTo(availableColors.Select(x => "#" + x.HexString)).Retry(),
                "Cannot find all available colors in the selection list");
            selectList.SelectByValue("#" + colorToSelect.HexString);

            this.Page.NameInput.SendKeys(myName);
            this.Page.ParticipantPassphraseInput.SendKeys("secret");
            this.Page.Submit();

            // Then
            Assert.That(() => new SelectElement(secondInstance.ColorSelect).Options.Select(x => x.GetAttribute("value")), Does.Not.Contains("#" + colorToSelect.HexString).And.Not.EquivalentTo(availableColors.Select(x => "#" + x.HexString)).Retry());
        }

        [Test]
        public async Task JoinRetrospectivePage_KnownRetrospective_JoinAsFacilitatorUpdatesParticipantListInRealtime() {
            // Given
            string retroId = await this.CreateRetrospective("scrummaster", "secret");
            string myName = Name.Create();
            this.Page.Navigate(this.App, retroId);

            var secondInstance = this.App.CreatePageObject<JoinRetrospectivePage>().RegisterAsTestDisposable();
            secondInstance.Navigate(this.App, retroId);

            // When
            this.Page.NameInput.SendKeys(myName);
            new SelectElement(this.Page.ColorSelect).SelectByIndex(2);
            this.Page.IsFacilitatorCheckbox.Click();
            this.Page.WebDriver.Retry(_ => {
                this.Page.FacilitatorPassphraseInput.SendKeys("scrummaster");
                return true;
            });
            this.Page.Submit();

            Thread.Sleep(500);

            using IServiceScope scope = this.App.CreateTestServiceScope();
            scope.SetNoAuthenticationInfo();
            ParticipantsInfoList participants = await scope.Send(new GetParticipantsInfoQuery(retroId));
            ParticipantInfo facilitator = participants.Participants.First(x => x.Name == myName);

            // Then
            Assert.That(() => secondInstance.OnlineList.OnlineListItems.Select(x => x.Text), Has.One.Contains(myName));
            Assert.That(() => secondInstance.OnlineList.GetListItem(facilitator.Id).FindElements(By.ClassName("fa-crown")), Is.Not.Empty.Retry());
        }

        private Task SetRetrospective(string retroId, Action<Retrospective> action) {
            using IServiceScope scope = this.App.CreateTestServiceScope();
            return scope.SetRetrospective(retroId, action);
        }
        private async Task<string> CreateRetrospective(string facilitatorPassword, string password) {
            var command = new CreateRetrospectiveCommand {
                Title = TestContext.CurrentContext.Test.FullName,
                FacilitatorPassphrase = facilitatorPassword,
                Passphrase = password
            };

            this.ServiceScope.SetNoAuthenticationInfo();

            CreateRetrospectiveCommandResponse result = await this.ServiceScope.Send(command);
            return result.Identifier.StringId;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dynamically instantiated")]
    public sealed class JoinRetrospectivePage : PageObject {
        public IWebElement Title => this.WebDriver.FindElement(By.CssSelector("h1.title"));
        public IWebElement NameInput => this.WebDriver.FindElement(By.Id("retro-name"));
        public IWebElement FacilitatorPassphraseInput => this.WebDriver.FindElement(By.Id("retro-facilitator-passphrase"));
        public IWebElement ParticipantPassphraseInput => this.WebDriver.FindElement(By.Id("retro-passphrase"));
        public IWebElement ColorInput => this.WebDriver.FindElement(By.Id("retro-color"));
        public IWebElement ColorSelect => this.WebDriver.FindElement(By.Id("retro-color-choices"));
        public IWebElement IsFacilitatorCheckbox => this.WebDriver.FindElement(By.Id("retro-is-facilitator"));
        public IWebElement SubmitButton => this.WebDriver.FindVisibleElement(By.Id("join-retro-button"));
        public void Submit() => this.SubmitButton.Click();

        public ReadOnlyCollection<IWebElement> GetValidationMessages() => this.WebDriver.FindElements(By.ClassName("validation-message"));
        public void Navigate(ReturnAppFactory app, string retroId) => this.WebDriver.NavigateToBlazorPage(app.CreateUri($"retrospective/{retroId}/join"));

        public RetrospectiveOnlineListComponent OnlineList => new RetrospectiveOnlineListComponent(this.WebDriver);
    }
}
