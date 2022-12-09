// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;
using NUnit.Framework;

[TestFixture]
public class CreateRetrospectiveTests : PageFixture<CreateRetrospectivePage> {
    [Test]
    public async Task CreateRetrospective_SubmitWithoutValidation_ShowsValidationMessages() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.Submit();

        // Then
        await this.Page.EnsureValidationMessages();
        string[] messages = await this.Page.GetValidationMessages();

        Assert.That(messages, Has.One.Contain("'Title' must not be empty"));
        Assert.That(messages, Has.One.Contain("'Facilitator Passphrase' must not be empty"));
    }

    [Test]
    public async Task CreateRetrospective_SubmitValidWithBothPassphrases_ShowQrCodeAndLink() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.RetrospectiveTitleInput.FillAsync("...");
        await this.Page.RetrospectiveTitleInput.FillAsync(TestContext.CurrentContext.Test.Name);
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");
        await this.Page.ParticipantPassphraseInput.FillAsync("the participator password");

        await this.Page.Submit();

        // Then
        await this.Page.UrlLocationInput.Expected().ToBeVisibleAsync();
        await this.Page.UrlLocationInput.Expected().ToHaveValueAsync(new Regex(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));

        await this.Page.FacilitatorInstructions.Expected().ToContainTextAsync("my secret facilitator password");
        await this.Page.ParticipatorInstructions.Expected().ToContainTextAsync("the participator password");
    }

    [Test]
    public async Task CreateRetrospective_SubmitValidWithOnlyFacilitatorPassphrase_ShowQrCodeAndLink() {
        // Given
        await this.Page.Navigate(this.App);

        // When
        await this.Page.RetrospectiveTitleInput.FillAsync(TestContext.CurrentContext.Test.FullName);
        await this.Page.FacilitatorPassphraseInput.FillAsync("my secret facilitator password");

        await this.Page.Submit();

        // Then
        await this.Page.UrlLocationInput.Expected().ToHaveValueAsync(new Regex(@"http://localhost:\d+/retrospective/([A-z0-9]+)/join"));

        await this.Page.FacilitatorInstructions.Expected().ToContainTextAsync("my secret facilitator password");
        await this.Page.ParticipatorInstructions.Expected().ToContainTextAsync("no password is required");
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dynamically instantiated")]
public sealed class CreateRetrospectivePage : PageObject {
    public ILocator RetrospectiveTitleInput => this.BrowserPage.Locator("#retro-title");
    public ILocator FacilitatorPassphraseInput => this.BrowserPage.Locator("#retro-facilitator-passphrase");
    public ILocator ParticipantPassphraseInput => this.BrowserPage.Locator("#retro-passphrase");
    public ILocator SubmitButton => this.BrowserPage.Locator("#create-retro-button");
    public ILocator ModalSubmitButton => this.BrowserPage.Locator("#modal-create-retro-button");

    public ILocator UrlLocationInput => this.BrowserPage.Locator("#retro-location");
    public ILocator ParticipatorInstructions => this.BrowserPage.FindElementByTestElementId("participator-instructions");
    public ILocator FacilitatorInstructions => this.BrowserPage.FindElementByTestElementId("facilitator-instructions");

    public ILocator LobbyCreationPassphraseInput => this.BrowserPage.Locator("#retro-lobby-creation-passphrase");
    public ILocator LobbyCreationPassphraseModal => this.BrowserPage.FindElementByTestElementId("lobby-creation-passphrase-modal");

    public ILocator LobbyPassphraseModal => this.BrowserPage.Locator("[data-test-element-id=\"lobby-creation-passphrase-modal\"]");

    public async Task Navigate(ReturnAppFactory app)
    {
        await this.BrowserPage.GotoBlazorPageAsync(app.CreateUri("create-retro"));
        await this.RetrospectiveTitleInput.Expected().ToBeVisibleAsync();
    }

    public Task Submit() => this.SubmitButton.ClickAsync();
    public Task ModalSubmit() => this.ModalSubmitButton.ClickAsync();

    public Task EnsureValidationMessages() => this.BrowserPage.Locator(".validation-message").First.Expected().ToBeVisibleAsync();
    public async Task<string[]> GetValidationMessages()
    {
        ILocator locator = this.BrowserPage.Locator(".validation-message");

        int count = await locator.CountAsync();
        string[] msg = new string[count];
        for (int i = 0; i < count; i++)
        {
            msg[i] = await locator.Nth(i).TextContentAsync();
        }

        return msg;
    }
}
