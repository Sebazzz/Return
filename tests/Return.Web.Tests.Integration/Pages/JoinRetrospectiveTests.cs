// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Common;
    using Domain.Services;
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
        public IWebElement SubmitButton => this.WebDriver.FindVisibleElement(By.Id("join-retro-button"));
        public void Submit() => this.SubmitButton.Click();

        public ReadOnlyCollection<IWebElement> GetValidationMessages() => this.WebDriver.FindElements(By.ClassName("validation-message"));
        public void Navigate(ReturnAppFactory app, string retroId) => this.WebDriver.NavigateToBlazorPage(app.CreateUri($"retrospective/{retroId}/join"));
    }
}
