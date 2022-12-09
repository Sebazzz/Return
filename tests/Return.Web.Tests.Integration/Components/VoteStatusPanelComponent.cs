// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteStatusPanelComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;

public sealed class VoteStatusPanelComponent {
    private readonly ILocator _locator;

    public VoteStatusPanelComponent(ILocator locator) {
        this._locator = locator;
    }

    public ILocator VoteStatusPerParticipantLocator(int id) =>  this._locator.FindElementByTestElementId("participant-vote-status", id);
    public Task<List<VoteStatusForParticipant>> VoteStatusPerParticipant() =>  this._locator.GenerateSubElementsByTestElementId("participant-vote-status",VoteStatusForParticipant.Create);
}

public sealed class VoteStatusForParticipant {
    private readonly ILocator _locator;

    private VoteStatusForParticipant(ILocator locator) {
        this._locator = locator;
    }

    public static async Task<VoteStatusForParticipant> Create(ILocator locator) =>
        new(locator)
        {
            ParticipantId = await locator.GetAttributeAsync<int>("data-id")
        };

    public int ParticipantId { get; init; }

    public ILocator CastVotes => this._locator.FindElementByTestElementId("cast-vote");
    public ILocator UncastVotes => this._locator.FindElementByTestElementId("uncast-vote");
    public ILocator TotalVotes => this._locator.Locator(".vote-indicator");
}
