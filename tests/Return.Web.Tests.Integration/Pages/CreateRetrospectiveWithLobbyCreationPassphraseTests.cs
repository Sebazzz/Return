// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveWithLobbyCreationPassphraseTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Application.Common.Settings;
    using Common;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public sealed class CreateRetrospectiveWithLobbyCreationPassphraseTests : PageFixture<CreateRetrospectivePage> {
        private static readonly string SecurityPassword = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        private TemporarySettingsScope<SecuritySettings> _temporarySettingsScope;

        [SetUp]
        public void SetSecuritySettings() {
            this._temporarySettingsScope = new TemporarySettingsScope<SecuritySettings>(this.App);
            this._temporarySettingsScope.SaveSettings(securitySettings => {
                securitySettings.LobbyCreationPassphrase = SecurityPassword;
            });
        }

        [TearDown]
        public void ResetSecuritySettings() => this._temporarySettingsScope.RestoreSettings();

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSession_SubmitValid_ShowDialog() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.RetrospectiveTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName);
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            this.Page.Submit();

            // Then
            Assert.That(this.Page.LobbyCreationPassphraseModal, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                "Expected modal for the passphrase to become visible");
        }

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSession_PasswordDialog_InvalidPassphrase_ShowErrorInsideDialog() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.RetrospectiveTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName);
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            this.Page.Submit();
            this.EnsurePasswordDialogVisible();

            this.Page.LobbyCreationPassphraseInput.SendKeys("invalid password");
            this.Page.ModalSubmit();

            // Then
            string[] messages = new DefaultWait<CreateRetrospectivePage>(this.Page)
                .Until(p => {
                    ReadOnlyCollection<IWebElement> collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contains("Invalid pre-shared passphrase entered needed for creating a retrospective"));

            Assert.That(() => this.Page.LobbyCreationPassphraseModal.Displayed, Is.True.Retry(),
                "Expected the modal to become and stay visible because the validation error is shown inside the modal");
        }

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitValidPassphrase_CreateSession() {
            // Given
            this.Page.Navigate(this.App);

            // When
            this.Page.RetrospectiveTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName);
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            this.Page.Submit();
            this.EnsurePasswordDialogVisible();

            this.Page.LobbyCreationPassphraseInput.SendKeys(SecurityPassword);
            this.Page.ModalSubmit();

            // Then
            Assert.That(this.Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));
        }
        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitWithValidPassphraseWithFormErrors_HidesModal() {
            // Given
            this.Page.Navigate(this.App);

            // When
            // ... (don't enter a title, which is a required field)
            this.Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            this.Page.Submit();
            this.EnsurePasswordDialogVisible();

            this.Page.LobbyCreationPassphraseInput.SendKeys(SecurityPassword);
            this.Page.ModalSubmit();

            // Then
            Assert.That(() => this.Page.LobbyCreationPassphraseModalIsDisplayed, Is.False.Retry(),
                "Expected the modal to become hidden because the error is on the form itself");
        }

        private void EnsurePasswordDialogVisible() =>
            Assume.That(this.Page.LobbyCreationPassphraseModal,
                Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                "Expected modal for the passphrase to become visible");
    }
}
