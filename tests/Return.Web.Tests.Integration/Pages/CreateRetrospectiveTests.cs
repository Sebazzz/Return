// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public class CreateRetrospectiveTests : PageFixture<CreateRetrospectivePage> {
        [Test]
        public void CreateRetrospective_SubmitWithoutValidation_ShowsValidationMessages() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.Submit();

            // Then
            string[] messages = new DefaultWait<CreateRetrospectivePage>(this.Page)
                .Until(p => {
                    ReadOnlyCollection<IWebElement> collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contain("'Title' must not be empty"));
            Assert.That(messages, Has.One.Contain("'Facilitator Passphrase' must not be empty"));
        }

        [Test]
        public void CreateRetrospective_SubmitValidWithBothPassphrases_ShowQrCodeAndLink() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.RetrospectiveTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName);
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");
            this.Page.ParticipantPassphraseInput.SendKeys("the participator password");

            this.Page.Submit();

            // Then
            Assert.That(this.Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));

            Assert.That(this.Page.FacilitatorInstructions.Text, Contains.Substring("my secret facilitator password"));
            Assert.That(this.Page.ParticipatorInstructions.Text, Contains.Substring("the participator password"));
        }

        [Test]
        public void CreateRetrospective_SubmitValidWithOnlyFacilitatorPassphrase_ShowQrCodeAndLink() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.RetrospectiveTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName);
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            this.Page.Submit();

            // Then
            Assert.That(this.Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));

            Assert.That(this.Page.FacilitatorInstructions.Text, Contains.Substring("my secret facilitator password"));
            Assert.That(this.Page.ParticipatorInstructions.Text, Contains.Substring("no password is required"));
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dynamically instantiated")]
    public sealed class CreateRetrospectivePage : PageObject {
        public IWebElement RetrospectiveTitleInput => this.WebDriver.FindVisibleElement(By.Id("retro-title"));
        public IWebElement FacilitatorPassphraseInput => this.WebDriver.FindVisibleElement(By.Id("retro-facilitator-passphrase"));
        public IWebElement ParticipantPassphraseInput => this.WebDriver.FindVisibleElement(By.Id("retro-passphrase"));
        public IWebElement SubmitButton => this.WebDriver.FindVisibleElement(By.Id("create-retro-button"));

        public IWebElement UrlLocationInput => this.WebDriver.FindVisibleElement(By.Id("retro-location"));
        public IWebElement ParticipatorInstructions => this.WebDriver.FindElementByTestElementId("participator instructions");
        public IWebElement FacilitatorInstructions => this.WebDriver.FindElementByTestElementId("facilitator instructions");

        public void Navigate(ReturnAppFactory app) => this.WebDriver.NavigateToBlazorPage(app.CreateUri("create-retro"));
        public void Submit() => this.SubmitButton.Click();

        public string GetUrlShown() => this.WebDriver.Retry(_ => this.UrlLocationInput.GetAttribute("value"));
        public ReadOnlyCollection<IWebElement> GetValidationMessages() => this.WebDriver.FindElements(By.ClassName("validation-message"));
    }
}
