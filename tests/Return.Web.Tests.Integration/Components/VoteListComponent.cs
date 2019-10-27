// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteListComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components {
    using System;
    using System.Collections.ObjectModel;
    using Common;
    using OpenQA.Selenium;

    public sealed class VoteListComponent {
        private readonly IWebElement _webElement;

        public VoteListComponent(IWebElement webElement) {
            this._webElement = webElement;
        }

        public int Id => this._webElement.GetAttribute<int>("data-id");
        public IWebElement VoteButton => this._webElement.FindElement(By.ClassName("vote-list__vote-button"));
        public ReadOnlyCollection<IWebElement> Votes => this._webElement.FindElements(By.ClassName("vote-indicator"));
        public bool IsVoteButtonEnabled => !this.VoteButton.GetAttribute("class").Contains("vote-list__vote-button--disabled", StringComparison.OrdinalIgnoreCase);

        public void ClickVoteButton() {
            this.VoteButton.Click();
        }
    }
}
