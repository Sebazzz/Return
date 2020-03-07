// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobby.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System.Collections.ObjectModel;
    using System.Threading;
    using Common;
    using Components;
    using Domain.Entities;
    using OpenQA.Selenium;

    public sealed class RetrospectiveLobby : PageObject {
        public IWebElement TimeInMinutesInput => this.WebDriver.FindElementByTestElementId("time-in-minutes-input");
        public IWebElement VoteCountInput => this.WebDriver.FindElementByTestElementId("vote-count-input");
        public IWebElement WorkflowContinueButton => this.WebDriver.FindElementByTestElementId("workflow-continue-button");
        public IWebElement ToggleViewButton => this.WebDriver.FindElementByTestElementId("toggle-view-button");
        public IWebElement TimerText => this.WebDriver.FindElementByTestElementId("timer");
        public ReadOnlyCollection<IWebElement> NoteLaneElements => this.WebDriver.FindElementsByTestElementId("note-lane");
        public NoteLaneComponent GetLane(KnownNoteLane id) => new NoteLaneComponent(this.WebDriver.FindElementByTestElementId("note-lane", (int)id));
        public VoteStatusPanelComponent VoteStatus => new VoteStatusPanelComponent(this.WebDriver.FindVisibleElement(By.ClassName("vote-status-panel")));

        public void InvokeContinueWorkflow() {
            this.WorkflowContinueButton.Click();

            // Insert sleep for AppVeyor and slower CI
            Thread.Sleep(1000);
        }
    }
}
