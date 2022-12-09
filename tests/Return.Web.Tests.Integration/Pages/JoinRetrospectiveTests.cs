// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
using Application.Retrospectives.Commands.CreateRetrospective;
using Application.Retrospectives.Queries.GetParticipantsInfo;
using Common;
using Components;
using Domain.Entities;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;

[TestFixture]
public sealed class JoinRetrospectiveTests : PageFixture<JoinRetrospectivePage> {
    [Test]
    public async Task JoinRetrospectivePage_UnknownRetrospective_ShowNotFoundMessage() {
        // Given
        string retroIdentifier = new RetroIdentifierService().CreateNew().StringId;

        // When
        await this.Page.Navigate(this.App, retroIdentifier);

        // Then
        await this.Page.Title.Expected().ToHaveTextAsync(new Regex("not found"));
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospective_FormShownWithValidation() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        await this.Page.Navigate(this.App, retroId);

        // When
        await this.Page.Submit();

        // Then
        string[] messages = await this.Page.GetValidationMessages();

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
        await this.Page.Navigate(this.App, retroId);

        // Then
        await this.Page.InfoNotification.Expected().ToHaveCountAsync(1);
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospectiveFinished_ShowMessage() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        await this.SetRetrospective(retroId, retro => retro.CurrentStage = RetrospectiveStage.Finished);

        // When
        await this.Page.Navigate(this.App, retroId);

        // Then
        await this.Page.WarningNotification.Expected().ToHaveCountAsync(1);
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospective_ValidatesParticipantPassphaseAndRedirectsToLobby() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        string myName = Name.Create();
        await this.Page.Navigate(this.App, retroId);

        // When
        await this.Page.NameInput.FillAsync(myName);
        await this.Page.ColorSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await this.Page.ParticipantPassphraseInput.FillAsync("secret");
        await this.Page.Submit();

        // Then
        await this.Page.BrowserPage.Expected().ToHaveURLAsync(new Regex("/retrospective/" + retroId + "/lobby"));
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospective_JoinParticipantUpdatesParticipantListInRealtime() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        string myName = Name.Create();
        await this.Page.Navigate(this.App, retroId);

        JoinRetrospectivePage secondInstance = (await this.App.CreatePageObject<JoinRetrospectivePage>()).RegisterAsTestDisposable();
        await secondInstance.Navigate(this.App, retroId);

        // When
        await this.Page.NameInput.FillAsync(myName);
        await this.Page.ColorSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await this.Page.ParticipantPassphraseInput.FillAsync("secret");
        await this.Page.Submit();

        // Then
        await secondInstance.OnlineList.OnlineListItems.Expected().ToHaveTextAsync(myName);
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospective_JoinParticipantUpdatesColorListInRealtime() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        string myName = Name.Create();
        await this.Page.Navigate(this.App, retroId);

        JoinRetrospectivePage secondInstance = (await this.App.CreatePageObject<JoinRetrospectivePage>()).RegisterAsTestDisposable();
        await secondInstance.Navigate(this.App, retroId);

        IList<AvailableParticipantColorModel> availableColors;
        {
            using IServiceScope scope = this.App.CreateTestServiceScope();
            scope.SetNoAuthenticationInfo();
            availableColors = await scope.Send(new GetAvailablePredefinedParticipantColorsQuery(retroId));
        }
        AvailableParticipantColorModel colorToSelect = availableColors[TestContext.CurrentContext.Random.Next(0, availableColors.Count)];

        // When
        foreach (AvailableParticipantColorModel availableColor in availableColors) {
            await this.Page.ColorSelect.SelectOptionAsync(new SelectOptionValue { Label = availableColor.Name });
        }
        await this.Page.ColorSelect.SelectOptionAsync(new SelectOptionValue { Value = "#" + colorToSelect.HexString });

        await this.Page.NameInput.FillAsync(myName);
        await this.Page.ParticipantPassphraseInput.FillAsync("secret");
        await this.Page.Submit();

        // Then
        Assert.That(() => secondInstance.ColorSelect.Locator($"option[value=\"{colorToSelect.HexString}\"]").CountAsync().GetAwaiter().GetResult(),
            Is.EqualTo(0).Retry());
    }

    [Test]
    public async Task JoinRetrospectivePage_KnownRetrospective_JoinAsFacilitatorUpdatesParticipantListInRealtime() {
        // Given
        string retroId = await this.CreateRetrospective("scrummaster", "secret");
        string myName = Name.Create();
        await this.Page.Navigate(this.App, retroId);

        JoinRetrospectivePage secondInstance = (await this.App.CreatePageObject<JoinRetrospectivePage>()).RegisterAsTestDisposable();
        await secondInstance.Navigate(this.App, retroId);

        // When
        await this.Page.NameInput.FillAsync(myName);
        await this.Page.ColorSelect.SelectOptionAsync(new SelectOptionValue { Index = 2 });
        await this.Page.IsFacilitatorCheckbox.CheckAsync();
        await this.Page.FacilitatorPassphraseInput.FillAsync("scrummaster");
        await this.Page.Submit();

        Thread.Sleep(500);

        using IServiceScope scope = this.App.CreateTestServiceScope();
        scope.SetNoAuthenticationInfo();
        ParticipantsInfoList participants = await scope.Send(new GetParticipantsInfoQuery(retroId));
        ParticipantInfo facilitator = participants.Participants.First(x => x.Name == myName);

        // Then
        await secondInstance.OnlineList.GetListItem(facilitator.Id).Locator(".fa-crown").Expected().ToBeVisibleAsync();
        Assert.That(() => secondInstance.OnlineList.OnlineListItems.AllInnerTextsAsync().GetAwaiter().GetResult(), Has.One.Contains(myName));
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
    public ILocator Title => this.BrowserPage.Locator("h1.title");
    public ILocator NameInput => this.BrowserPage.Locator("#retro-name");
    public ILocator FacilitatorPassphraseInput => this.BrowserPage.Locator("#retro-facilitator-passphrase");
    public ILocator ParticipantPassphraseInput => this.BrowserPage.Locator("#retro-passphrase");
    public ILocator ColorInput => this.BrowserPage.Locator("#retro-color");
    public ILocator ColorSelect => this.BrowserPage.Locator("#retro-color-choices");
    public ILocator IsFacilitatorCheckbox => this.BrowserPage.Locator("#retro-is-facilitator");
    public ILocator InfoNotification => this.BrowserPage.Locator(".notification.is-info");
    public ILocator WarningNotification => this.BrowserPage.Locator(".notification.is-warning");
    public ILocator SubmitButton => this.BrowserPage.Locator("#join-retro-button");

    public Task Submit() => this.SubmitButton.ClickAsync();

    public async Task Navigate(ReturnAppFactory app, string retroId)
    {
        await this.BrowserPage.GotoBlazorPageAsync(app.CreateUri($"retrospective/{retroId}/join"));
        await this.Title.Expected().ToBeVisibleAsync();
    }

    public RetrospectiveOnlineListComponent OnlineList => new(this.BrowserPage);

    public async Task<string[]> GetValidationMessages() {
        ILocator locator = this.BrowserPage.Locator(".validation-message");

        await locator.First.Expected().ToBeVisibleAsync();

        int count = await locator.CountAsync();
        string[] msg = new string[count];
        for (int i = 0; i < count; i++)
        {
            msg[i] = await locator.Nth(i).TextContentAsync();
        }

        return msg;
    }
}
