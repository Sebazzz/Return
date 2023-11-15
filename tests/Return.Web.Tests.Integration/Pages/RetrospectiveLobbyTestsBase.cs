// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
//
//  File:           : RetrospectiveLobbyTestsBase.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************
namespace Return.Web.Tests.Integration.Pages;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;

public class RetrospectiveLobbyTestsBase : TwoClientPageFixture<RetrospectiveLobby> {
    protected string RetroId { get; set; }
    private int _colorIndex = 1;

    [SetUp]
    public void ResetColorIndex() => this._colorIndex = 1;

    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Retry runs while the webdriver runs")]
    protected async Task Join(RetrospectiveLobby pageObject, bool facilitator, string name = null, bool alreadyJoined = false, string colorName = null, Func<Task> submitCallback = null) {
        using var joinPage = new JoinRetrospectivePage();
        joinPage.InitializeFrom(pageObject);
        await joinPage.Navigate(this.App, this.RetroId);

        await joinPage.NameInput.FocusAsync();
        await joinPage.NameInput.Expected().ToBeVisibleAsync();

        await joinPage.NameInput.FillAsync(name ?? Name.Create());

        if (!alreadyJoined) {
            if (colorName != null) {
                TestContext.WriteLine($"Selecting dropdown option: {colorName}");
                await joinPage.ColorSelect.SelectOptionAsync(new SelectOptionValue { Label = colorName /*Partial match?*/});
            }
            else {
                TestContext.WriteLine($"Selecting dropdown index: {this._colorIndex}");
                await joinPage.ColorSelect.SelectOptionAsync(new SelectOptionValue { Index = this._colorIndex++ });
            }
        }
        else {
            // Force refresh
            await joinPage.Unfocus();
        }

        if (facilitator) {
            if (!alreadyJoined) {
                await joinPage.IsFacilitatorCheckbox.ClickAsync();
            }

            await joinPage.FacilitatorPassphraseInput.FillAsync("scrummaster");
        }

        Task t = submitCallback?.Invoke();
        if (t is not null) await t;

        await joinPage.Submit();
    }

    protected Task WaitNavigatedToLobby() => Task.WhenAll(WaitNavigatedToLobby(this.Client1), WaitNavigatedToLobby(this.Client2));

    protected async Task SetRetrospective(Action<Retrospective> action) {
        using IServiceScope scope = this.App.CreateTestServiceScope();
        await scope.SetRetrospective(this.RetroId, action);
    }

    private static async Task WaitNavigatedToLobby(RetrospectiveLobby pageObject) {
        Stopwatch sw = new();
        sw.Start();

        await pageObject.BrowserPage.Expected().ToHaveURLAsync(new Regex("/lobby"));
        await pageObject.BrowserPage.FindElementByTestElementId("main-board").Expected().ToBeVisibleAsync(); // If this fails: The retrospective board does not load

        sw.Stop();
        TestContext.WriteLine($"Navigated to lobby in {sw.Elapsed}");

        await Task.Delay(500);
    }
}
