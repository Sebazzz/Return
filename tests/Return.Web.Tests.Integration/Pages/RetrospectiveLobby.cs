// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobby.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System.Threading.Tasks;
using Common;
using Components;
using Domain.Entities;
using Microsoft.Playwright;

public sealed class RetrospectiveLobby : PageObject {
    public ILocator TimeInMinutesInput => this.BrowserPage.FindElementByTestElementId("time-in-minutes-input");
    public ILocator VoteCountInput => this.BrowserPage.FindElementByTestElementId("vote-count-input");
    public ILocator WorkflowContinueButton => this.BrowserPage.FindElementByTestElementId("workflow-continue-button");
    public ILocator ToggleViewButton => this.BrowserPage.FindElementByTestElementId("toggle-view-button");
    public ILocator TimerText => this.BrowserPage.FindElementByTestElementId("timer");
    public ILocator NoteLaneElements => this.BrowserPage.FindElementByTestElementId("note-lane");
    public NoteLaneComponent GetLane(KnownNoteLane id) => new(this.BrowserPage.FindElementByTestElementId("note-lane", (int)id));
    public VoteStatusPanelComponent VoteStatus => new(this.BrowserPage.Locator(".vote-status-panel"));

    public async Task InvokeContinueWorkflow()
    {
        await this.WorkflowContinueButton.Expected().ToBeVisibleAsync();
        await this.WorkflowContinueButton.Expected().ToBeEnabledAsync();
        await this.WorkflowContinueButton.ClickAsync();
    }
}
