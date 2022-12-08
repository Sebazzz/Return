// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveWithLobbyCreationPassphraseTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Common.Settings;
using Common;
using NUnit.Framework;

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
    public async Task LobbyCreationPassphraseActive_CreatePokerSession_SubmitValid_ShowDialog() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.RetrospectiveTitleInput.FillAsync(TestContext.CurrentContext.Test.FullName);
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");

        await this.Page.Submit();

        // Then
        await this.Page.LobbyCreationPassphraseModal.Expected().ToBeVisibleAsync();
    }

    [Test]
    public async Task LobbyCreationPassphraseActive_CreatePokerSession_PasswordDialog_InvalidPassphrase_ShowErrorInsideDialog() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.RetrospectiveTitleInput.FillAsync(TestContext.CurrentContext.Test.FullName);
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");

        await this.Page.Submit();
        await this.EnsurePasswordDialogVisible();

        await this.Page.LobbyCreationPassphraseInput.FillAsync("invalid password");
        await this.Page.ModalSubmit();

        // Then
        string[] messages = await this.Page.GetValidationMessages();

        Assert.That(messages, Has.One.Contains("Invalid pre-shared passphrase entered needed for creating a retrospective"));

        await this.Page.LobbyCreationPassphraseModal.Expected().ToBeVisibleAsync();
    }

    [Test]
    public async Task LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitValidPassphrase_CreateSession() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.RetrospectiveTitleInput.FillAsync(TestContext.CurrentContext.Test.FullName);
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");

        await  this.Page.Submit();
        await  this.EnsurePasswordDialogVisible();

        await  this.Page.LobbyCreationPassphraseInput.FillAsync(SecurityPassword);
        await  this.Page.ModalSubmit();

        // Then
        await this.Page.UrlLocationInput.Expected().
            ToHaveValueAsync(new Regex(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));
    }

    [Test]
    public async Task LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitWithValidPassphraseWithFormErrors_HidesModal() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        // ... (don't enter a title, which is a required field)
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");

        await this.Page.Submit();
        await this.EnsurePasswordDialogVisible();

        await this.Page.LobbyCreationPassphraseInput.FillAsync(SecurityPassword);
        await this.Page.ModalSubmit();

        // Then
        await this.Page.LobbyPassphraseModal.Expected().ToBeHiddenAsync();
    }

    private Task EnsurePasswordDialogVisible()
    {
        Assume.That(() => this.Page.LobbyCreationPassphraseModal.IsVisibleAsync().GetAwaiter().GetResult(),
            Is.True.Retry(),
            "Expected modal for the passphrase to become visible");

        return Task.CompletedTask;
    }
        
}
