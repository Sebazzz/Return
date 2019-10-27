// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteStatusPanelComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using OpenQA.Selenium;

    public sealed class VoteStatusPanelComponent {
        private readonly IWebElement _webElement;

        public VoteStatusPanelComponent(IWebElement webElement) {
            this._webElement = webElement;
        }

        public IEnumerable<VoteStatusForParticipant> VoteStatusPerParticipant =>
            this._webElement.FindElementsByTestElementId("participant-vote-status").
                Select(x => new VoteStatusForParticipant(x));
    }

    public sealed class VoteStatusForParticipant {
        private readonly IWebElement _webElement;

        public VoteStatusForParticipant(IWebElement webElement) {
            this._webElement = webElement;
        }

        public int ParticipantId => this._webElement.GetAttribute<int>("data-id");

        public ReadOnlyCollection<IWebElement> CastVotes => this._webElement.FindElementsByTestElementId("cast-vote");
        public ReadOnlyCollection<IWebElement> UncastVotes => this._webElement.FindElementsByTestElementId("uncast-vote");
        public ReadOnlyCollection<IWebElement> TotalVotes => this._webElement.FindElements(By.ClassName("vote-indicator"));
    }
}
